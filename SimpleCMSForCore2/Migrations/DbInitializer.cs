using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleCMSForCore2.Models;

namespace SimpleCMSForCore2.Migrations
{
    public class DbInitializer
    {
        public static async  void Initialize(IServiceProvider services)
        {
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                context.Database.EnsureCreated();
                if (context.Categories.Any())
                {
                    foreach (var c in context.Categories)
                    {
                        context.Categories.Remove(c);
                    }
                    await context.SaveChangesAsync();
                }
                context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('T_Category', RESEED, 10000)");

                context.Database.ExecuteSqlCommand(@"
    IF OBJECT_ID(N'trg_CategoryInsert', N'TR') IS NOT NULL  
        DROP TRIGGER trg_CategoryInsert;  
");

                context.Database.ExecuteSqlCommand(@"
    CREATE TRIGGER [dbo].[trg_CategoryInsert] 
       ON  [dbo].[T_Category]
       FOR INSERT
    AS 
    BEGIN

        DECLARE @numrows int
        SET @numrows = @@ROWCOUNT

        if @numrows > 1
        BEGIN
            RAISERROR('只支持单行插入。', 16, 1)
            ROLLBACK TRAN
        END
        ELSE
        BEGIN
            UPDATE
                E
            SET
                HierarchyLevel    =
                CASE
                    WHEN E.ParentId IS NULL THEN 0
                    ELSE Parent.HierarchyLevel + 1
                END,
                FullPath =
                CASE
                    WHEN E.ParentId IS NULL THEN '.'
                    ELSE Parent.FullPath
                END + CAST(E.Id AS nvarchar(10)) + '.'
                FROM
                    T_Category AS E
                INNER JOIN
                    inserted AS I ON I.Id = E.Id
                LEFT OUTER JOIN
                    T_Category AS Parent ON Parent.Id = E.ParentId
        END

    END
");

                context.Database.ExecuteSqlCommand(@"
    IF OBJECT_ID(N'trg_CategoryUpdate', N'TR') IS NOT NULL  
        DROP TRIGGER trg_CategoryUpdate;  
");

                context.Database.ExecuteSqlCommand(@"
    CREATE TRIGGER [dbo].[trg_CategoryUpdate]
       ON  [dbo].[T_Category]
       FOR Update
    AS 
    BEGIN
      IF @@ROWCOUNT = 0
            RETURN

        if UPDATE(ParentId)
        BEGIN
            UPDATE
                E
            SET
                HierarchyLevel    =
                    E.HierarchyLevel - I.HierarchyLevel +
                        CASE
                            WHEN I.ParentId IS NULL THEN 0
                            ELSE Parent.HierarchyLevel + 1
                        END,
                FullPath =
                    ISNULL(Parent.FullPath, '.') +
                    CAST(I.Id as nvarchar(10)) + '.' +
                    RIGHT(E.FullPath, len(E.FullPath) - len(I.FullPath))
                FROM
                    T_Category AS E
                INNER JOIN
                    inserted AS I ON E.FullPath LIKE I.FullPath + '%'
                LEFT OUTER JOIN
                    T_Category AS Parent ON I.ParentId = Parent.Id
        END


    END
");

                await context.Categories.AddAsync(new Category()
                {
                    Title = "未分类",
                    Content = "未分类",
                    Created = DateTime.Now
                });
                await context.SaveChangesAsync();

                var name = "admin";
                var user = await userManager.FindByNameAsync(name);
                if (user != null) return;
                user = new ApplicationUser()
                {
                    UserName = name,
                    Email = "admin@amdin.com",
                    IsApprove = true,
                    Created = DateTime.Now
                };
                await userManager.CreateAsync(user, "abcd1234");
                await userManager.SetLockoutEnabledAsync(user, false);

                var roles = new string[] { "系统管理员", "编辑", "注册用户" };
                foreach (var c in roles)
                {
                    var record = new ApplicationRole() {Name = c};
                    if (!context.Roles.Any(m => m.Name.Equals(c)))
                    {
                        await roleManager.CreateAsync(record);
                    }
                    if (c.Equals("系统管理员"))
                    {
                        await userManager.AddToRoleAsync(user, record.Name);
                    }



                }
                await context.SaveChangesAsync();

                //await userManager.AddToRoleAsync(user, "系统管理员");

            }

        }

    }


}
