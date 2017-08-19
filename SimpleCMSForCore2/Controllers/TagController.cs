using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SimpleCMSForCore2.Helper;
using SimpleCMSForCore2.LocalResources;
using SimpleCMSForCore2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SimpleCMSForCore2.Controllers
{
    [Authorize]
    public class TagController : BaseController
    {
        private readonly JObject _allowSorts = new JObject()
        {
            { "Name", "Name" }
        };

        public TagController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            IHostingEnvironment hostingEnvironment) : base(context, signInManager, userManager, roleManager, 
            hostingEnvironment)
        {
        }

        public async Task<JObject> List(int? cid, string query, string sort, int? start, int? limit)
        {
            var hasTags = cid == null ? null : DbContext.Tags.Include(m=>m.ContentTags).Where(m => m.ContentTags.Any(n => n.ContentId == cid));
            var q = hasTags != null
                ? DbContext.Tags.Where(m => !hasTags.Select(n=>n.Id).Contains(m.Id))
                : DbContext.Tags.AsQueryable();
            if (!string.IsNullOrEmpty(query)) q = q.Where(m => m.Name.Contains(query));
            var total = await q.CountAsync();
            q = ExtJs.OrderBy(sort, _allowSorts, q);
            if (total > 0) q = ExtJs.Pagination(start ?? 0, limit ?? 0, total, q);
            if (q == null && hasTags == null) return ExtJs.WriterJObject(true, total: 0);
            if (q == null)
                ExtJs.WriterJObject(true, total: hasTags.Count(),
                    data: JArray.FromObject(hasTags.Select(m => new { Name = m.Name })));
            return ExtJs.WriterJObject(true, total: total,
                data:  JArray.FromObject(hasTags == null ? q.Select(m => new { Name = m.Name }) : q.Concat(hasTags).Select(m => new { Name = m.Name }))) ;
            
        }

        public async Task<JObject> Create(string value)
        {
            if (string.IsNullOrEmpty(value))
                return ExtJs.WriterJObject(false, errors: new JObject() { { "Name", Message.Required } });
            if (value.Length > 255)
                return ExtJs.WriterJObject(false,
                    errors: new JObject() { { "Name", string.Format(Message.MaxLength, 255) } });
            if (DbContext.Tags.Any(m => m.Name.Equals(value)))
                return ExtJs.WriterJObject(false,
                    errors: new JObject() { { "Name", string.Format(Message.Exists, Message.Tag, value) } });
            var q = new Tag() { Name = value };
            DbContext.Tags.Add(q);
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true);
        }

        public async Task<JObject> Delete(string[] id)
        {
            if (id == null) return ExtJs.WriterJObject(false, msg: Message.DeletedNotExist);
            var q = DbContext.Tags.Where(m => id.Contains(m.Name));
            var msgList = new List<string>();
            foreach (var c in q)
            {
                if (c.ContentTags.Any(m=>m.Content.State == 0))
                {
                    msgList.Add(string.Format(MessageListItem, "pointtwo", string.Format(Message.ForbidDelete, c.Name)));
                    continue;
                }
                msgList.Add(string.Format(MessageListItem, "pointthree", string.Format(Message.Deleted, Message.Tag, c.Name)));
                DbContext.Tags.Remove(c);
            }
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true, msg: string.Format(MessageList, string.Join("", msgList)));
        }

    }
}