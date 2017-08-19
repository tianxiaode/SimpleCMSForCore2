using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleCMSForCore2.Models;

namespace SimpleCMSForCore2.Controllers
{
    [Route("[controller]/[action]")]
    public class BaseController : Controller
    {
        #region Private Fields
        protected ApplicationDbContext DbContext;
        protected SignInManager<ApplicationUser> SignInManager;
        protected UserManager<ApplicationUser> UserManager;
        protected RoleManager<ApplicationRole> RoleManager;
        protected IHostingEnvironment HostintHostingEnvironment;
        #endregion Private Fields

        #region Constructor
        public BaseController(
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IHostingEnvironment hostingEnvironment)
        {
            // Dependency Injection
            DbContext = context;
            SignInManager = signInManager;
            UserManager = userManager;
            RoleManager = roleManager;
            HostintHostingEnvironment = hostingEnvironment;
        }
        #endregion Constructor

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var currentUser = await UserManager.FindByNameAsync(User.Identity.Name);
            return currentUser;
        }

        public readonly string MessageList = "<div class='message-tips'><ul class='message-tips-list'>{0}</ul></div>";
        public readonly string MessageListItem = "<li class='{0}'>{1}</li>";

    }
}