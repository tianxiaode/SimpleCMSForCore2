using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SimpleCMSForCore2.Helper;
using SimpleCMSForCore2.LocalResources;
using SimpleCMSForCore2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SimpleCMSForCore2.Controllers
{
    [Authorize(Roles = "系统管理员")]
    public class UserController : BaseController
    {
        private readonly JObject _allowSorts = new JObject()
        {
            { "UserName", "UserName" },
            { "Roles", "Roles" },
            { "IsApprove", "IsApprove" },
            { "LastLogin", "LastLogin" },
            { "Lockout", "Lockout" },
            { "Created", "Created" }
        };

        // GET: User
        public UserController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            IHostingEnvironment hostingEnvironment) : base(context, signInManager, userManager, roleManager, 
            hostingEnvironment)
        {
        }

        public async Task<JObject> List(string sort,int? limit, int? start, string query)
        {
            var q = DbContext.Users.Select(m => new
            {
                Id = m.Id,
                UserName = m.UserName,
                Roles = DbContext.Roles.Where(n=>DbContext.UserRoles.Where(l=>l.UserId == m.Id).Select(l=>l.RoleId).Contains(n.Id)).Select(n=>n.Name).SingleOrDefault(),
                Created = m.Created,
                LastLogin = m.LastLogin,
                Lockout = m.LockoutEnabled,
                IsApprove = m.IsApprove
            });
            if (!string.IsNullOrEmpty(query)) q = q.Where(m => m.UserName.Contains(query));
            var total = await q.CountAsync();
            if (total == 0) return ExtJs.WriterJObject(true, total: 0);
            q = ExtJs.OrderBy(sort, _allowSorts, q);
            q = ExtJs.Pagination(start ?? 0, limit ?? 0, total, q);
            var ja = new JArray();
            if (q == null) return ExtJs.WriterJObject(false, msg: Message.ListFailure);
            foreach (var c in q)
            {
                ja.Add(new JObject()
                {
                    { "Id",c.Id },
                    { "UserName",c.UserName },
                    { "Roles",  c.Roles},
                    { "Created", c.Created?.ToString(Message.DefaultDatetimeFormat)},
                    { "LastLogin", c.LastLogin?.ToString(Message.DefaultDatetimeFormat)},
                    { "Lockout", c.Lockout},
                    { "IsApprove", c.IsApprove}
                });
            }
            return ExtJs.WriterJObject(true, total: total, data: ja);
        }

        public async Task<JObject> CheckChange(int? id, string field)
        {
            if (id == null) return ExtJs.WriterJObject(false, msg: string.Format(Message.UpdatedNoExist, Message.User));
            if (!field.Equals("Lockout") && !field.Equals("IsApprove"))
                return ExtJs.WriterJObject(false, msg: Message.CheckChangeFieldNoExist);
            var q = await DbContext.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (q == null) return ExtJs.WriterJObject(false, msg: string.Format(Message.UpdatedNoExist, Message.User));
            if (field.Equals("Lockout")) q.LockoutEnabled = !q.LockoutEnabled;
            if (field.Equals("IsApprove")) q.IsApprove = !q.IsApprove;
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true);
        }

        public async Task<JObject> Delete(int[] id)
        {
            if(id == null) return ExtJs.WriterJObject(false , msg:Message.DeletedNotExist);
            var q = DbContext.Users.Where(m => id.Contains(m.Id));
            if(!await q.AnyAsync()) return ExtJs.WriterJObject(false, msg: Message.DeletedNotExist);
            var msgList = new List<string>();
            foreach (var c in q)
            {
                var roles =await UserManager.GetRolesAsync(c);
                await UserManager.RemoveFromRolesAsync(c, roles);
                c.UserProfiles.Clear();
                DbContext.Users.Remove(c);
                msgList.Add(string.Format(MessageListItem, "pointthree",
                    string.Format(Message.Deleted, Message.User, c.UserName)));
            }
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true, msg: string.Format(MessageList, string.Join("", msgList)));
        }

        [HttpPost]
        public async Task<JObject> Create(UserModel model)
        {
            if (string.IsNullOrEmpty(model.Password)) ModelState.AddModelError("Password", Message.Required);
            if (DbContext.Users.Any(m => m.UserName.Equals(model.UserName)))
                ModelState.AddModelError("UserName", string.Format(Message.Exists, Message.UserUserName, model.UserName));
            if (!ModelState.IsValid)
            {
                return ExtJs.WriterJObject(false, ExtJs.ModelStateToJObject(ModelState));
            }
            var user = new ApplicationUser
            {
                IsApprove = true,
                UserName = model.UserName,
                Created = DateTime.Now,
                Email = $"{model.UserName}@cms.com"
            };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return ExtJs.WriterJObject(false, msg: string.Join("<br/>", result.Errors));
            await UserManager.SetLockoutEnabledAsync(user, false);
            await UserManager.AddToRoleAsync(user, model.Roles);
            return ExtJs.WriterJObject(true);
        }

        [HttpPost]
        public async Task<JObject> Update(UserModel model)
        {
            if (!DbContext.Users.Any(m => m.UserName.Equals(model.UserName))) ModelState.AddModelError("UserName", string.Format(Message.UpdatedNoExist, model.UserName));
            if (!ModelState.IsValid)
            {
                return ExtJs.WriterJObject(false, ExtJs.ModelStateToJObject(ModelState));
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                await UserManager.ResetPasswordAsync(user, token, model.Password);
            }
            await DbContext.SaveChangesAsync();
            var roles = await UserManager.GetRolesAsync(user);
            await UserManager.RemoveFromRolesAsync(user, roles);
            await UserManager.AddToRoleAsync(user, model.Roles);
            return ExtJs.WriterJObject(true);
        }



    }
}