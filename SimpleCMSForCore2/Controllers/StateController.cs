using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using SimpleCMSForCore2.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleCMSForCore2.Helper;

namespace SimpleCMSForCore2.Controllers
{
    [Authorize]
    public class StateController : BaseController
    {
        // GET: State
        public StateController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            IHostingEnvironment hostingEnvironment) : base(context, signInManager, userManager, roleManager,
            hostingEnvironment)
        {
        }

        public async  Task<JObject> Save(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return ExtJs.WriterJObject(true);
            var user = await GetCurrentUserAsync();
            var q = await DbContext.UserProfiles.SingleOrDefaultAsync(m => m.Keyword.Equals(key) && m.UserProfileType == (byte)UserProfileType.State && m.UserId == user.Id);
            if (q == null)
            {
                DbContext.UserProfiles.Add(new UserProfile()
                {
                    Keyword = key,
                    Value = value,
                    UserProfileType = (byte)UserProfileType.State,
                    UserId = user.Id
                });
            }
            else
            {
                q.Value = value;
            }
            DbContext.SaveChanges();
            return ExtJs.WriterJObject(true);
        }

        public async Task<JObject> Restore()
        {
            var user = await GetCurrentUserAsync();
            var q = DbContext.UserProfiles.Where(m => m.UserProfileType == (byte)UserProfileType.State && m.UserId == user.Id).Select(m => new
            {
                Key = m.Keyword,
                Value = m.Value
            });
            return new JObject()
            {
                { "success", true },
                { "data", JArray.FromObject(q) }
            };
        }

    }
}