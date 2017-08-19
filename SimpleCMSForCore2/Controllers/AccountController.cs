using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleCMSForCore2.Helper;
using SimpleCMSForCore2.LocalResources;
using SimpleCMSForCore2.Models;

namespace SimpleCMSForCore2.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        // GET: Account

        public AccountController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            IHostingEnvironment hostingEnvironment) : base(context, signInManager, userManager, roleManager,
            hostingEnvironment)
        {
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<JObject> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return ExtJs.WriterJObject(false, errors: ExtJs.ModelStateToJObject(ModelState));
            }
            var verifyCode = HttpContext.Session.GetString("VerifyCode") ?? "";
            if (string.IsNullOrEmpty(model.VerifyCode) || !string.Equals(verifyCode, model.VerifyCode, StringComparison.CurrentCultureIgnoreCase))
            {
                return ExtJs.WriterJObject(false, errors: new JObject() { { "VerifyCode", Message.VerifyCode } });
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return ExtJs.WriterJObject(false, errors: new JObject()
                {
                    {"UserName", Message.SignInFailure} ,
                    {"Password", Message.SignInFailure}
                });
            }
            if (!user.IsApprove)
            {
                return ExtJs.WriterJObject(false, errors: new JObject()
                {
                    { "UserName", Message.IsApprove}
                });
            }
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe == "on",true);
            if (result.Succeeded)
            {
                user.LastLogin = DateTime.Now;
                await UserManager.UpdateAsync(user);
                return ExtJs.WriterJObject(true);
            }
            if (result.IsLockedOut)
            {
                return ExtJs.WriterJObject(false, errors: new JObject()
                {
                    new JProperty("UserName", Message.LockedOut)
                });
            }
            return ExtJs.WriterJObject(false, errors: new JObject()
            {
                new JProperty("UserName", Message.SignInFailure),
                new JProperty("Password", Message.SignInFailure)
            });
        }

        [AllowAnonymous]
        public async Task<JObject> UserInfo()
        {
            if (!User.Identity.IsAuthenticated) return ExtJs.WriterJObject(false);
            var user = await GetCurrentUserAsync();
            if (user == null) return ExtJs.WriterJObject(false);
            var roles = await UserManager.GetRolesAsync(user);
            return ExtJs.WriterJObject(true, data: new JObject()
            {
                {
                    "UserInfo", new JObject()
                    {
                        {"UserName", user.UserName},
                        {"Roles", JArray.FromObject(roles)}
                    }
                },
                {"Menu", GetMenu(roles.Contains("系统管理员"))}
            });
        }

        private JArray GetMenu(bool isAdmin)
        {
            //这里可以从数据库获取导航菜单返回
            var menus = new JArray()
            {
                new JObject(){
                    { "text" , "文章管理"},
                    { "iconCls" , "x-fa fa-file-text-o"},
                    { "rowCls" , "nav-tree-badge"},
                    { "viewType", "articleView" },
                    { "routeId", "articleview" },
                    { "leaf", true }
                },
                new JObject()
                {
                    { "text" , "媒体管理"},
                    { "iconCls" , "x-fa fa-file-image-o"},
                    { "rowCls" , "nav-tree-badge"},
                    { "viewType", "mediaView" },
                    { "routeId", "mediaView" },
                    { "leaf", true }
                }
            };
            if (isAdmin)
            {
                menus.Add(new JObject()
                {
                    { "text" , "用户管理"},
                    { "iconCls" , "x-fa fa-user"},
                    { "rowCls" , "nav-tree-badge"},
                    { "viewType", "userView" },
                    { "routeId", "userView" },
                    { "leaf", true }
                });
            }
            return menus;
        }

        public async Task<JObject>  LogOut()
        {
            await SignInManager.SignOutAsync();
            return ExtJs.WriterJObject(true);
        }

        public async Task<JObject> PasswordReset(PasswordResetModel model)
        {
            if (model.Password.Equals(model.NewPassword))
            {
                ModelState.AddModelError("Password", Message.OldPasswordEqualNew);
            }
            if (!ModelState.IsValid)
            {
                return ExtJs.WriterJObject(false, errors: ExtJs.ModelStateToJObject(ModelState));
            }
            var user = await GetCurrentUserAsync();
            var result = await UserManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (result.Succeeded)
            {
                await SignInManager.SignOutAsync();
                return ExtJs.WriterJObject(true);
            }
            else
            {
                return ExtJs.WriterJObject(false, errors: new JObject()
                {
                    { "Password", string.Join("<br/>", result.Errors)}
                });
            }
        }


    }
}