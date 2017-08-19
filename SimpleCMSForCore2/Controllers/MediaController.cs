using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using SimpleCMSForCore2.Helper;
using SimpleCMSForCore2.LocalResources;
using SimpleCMSForCore2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeDetective.Extensions;
using SimpleCMSForCore2.Models.Setting;

namespace SimpleCMSForCore2.Controllers
{
    [Authorize]
    public class MediaController : BaseController
    {
        // GET: Media

        public MediaController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            IHostingEnvironment hostingEnvironment, IOptions<Upload> upload) : base(context, signInManager, userManager, roleManager, 
            hostingEnvironment)
        {
            _upload = upload.Value;
        }

        private readonly Upload _upload;

        public async Task<JObject> Create(List<IFormFile> file)
        {
            var allowImageFileType = _upload.AllowImageFileType;
            var allowAudioFileType = _upload.AllowAudioFileType;
            var allowVideoFileType = _upload.AllowVideoFileType;
            var file1 = file.FirstOrDefault();
            if (file1 == null) return ExtJs.WriterJObject(false, msg: Message.NoFileUpload);
            var fileType = file1.OpenReadStream().GetFileType();
            var ext = fileType?.Extension;
            MediaType? type = null;
            if (allowImageFileType.IndexOf($",{ext},", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                type = MediaType.Image;

            }
            else if (allowAudioFileType.IndexOf($",{ext},", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                type = MediaType.Audio;
            }
            else if (allowVideoFileType.IndexOf($",{ext},", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                type = MediaType.Video;
            }
            if (type == null) return ExtJs.WriterJObject(false, msg: Message.FileTypeNotAllow + ext);

            var size = _upload.AllowUploadSize;
            if (file1.Length == 0 || file1.Length > size) return ExtJs.WriterJObject(false, msg: Message.FileSizeNotAllow);

            var guid = Guid.NewGuid();
            var filename = ShortGuid.ToShortGuid(guid);
            var path = $"{filename.Substring(0, 2)}/{filename.Substring(2, 2)}";
            var dir = ($"{HostintHostingEnvironment.WebRootPath}/upload/{path}");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var stream = new FileStream($"{dir}\\{filename}.{ext}", FileMode.Create);
            await file1.CopyToAsync(stream);

            var media = new Media()
            {
                Filename = $"{filename}.{ext}",
                Description = file1.FileName,
                Size = (int)file1.Length,
                Path = path,
                Type = (byte)type,
                Uploaded = DateTime.Now
            };
            DbContext.Mediae.Add(media);
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true, data: new JObject()
            {
                { "Id", media.Id },
                { "FileName" , media.Filename},
                { "Description" , media.Description},
                { "Size" , media.Size},
                { "Path" , media.Path },
                { "Type" ,media.Type },
                { "Uploaded",  media.Uploaded?.ToString(Message.DefaultDatetimeFormat)}

            });

        }

        private readonly JObject _allowSorts = new JObject()
        {
            { "Uploaded", "Uploaded" },
            { "Size", "Size" },
            { "Description", "Description" }
        };

        // GET: Media
        public async Task<JObject> List(string sort , int? start , int? limit, int[] type,int? year, int? month, int? day, string query)
        {
            if (type == null) return ExtJs.WriterJObject(true, total: 0);
            var q = DbContext.Mediae.Where(m=>type.Contains(m.Type));
            if (year != null && month !=null)
            {
                q = q.Where(m => m.Uploaded.Value.Year == year && m.Uploaded.Value.Month == month);
            }
            if (day !=null)
            {
                q = q.Where(m => m.Uploaded.Value.Day == day);
            }
            if (!string.IsNullOrEmpty(query)) q = q.Where(m => m.Description.Contains(query));
            var total = await q.CountAsync();
            if (total == 0) return ExtJs.WriterJObject(true, total: 0);
            q = ExtJs.OrderBy(sort, _allowSorts, q);
            q = ExtJs.Pagination(start ?? 0, limit ?? 0, total, q);
            var ja = new JArray();
            if (q == null) return ExtJs.WriterJObject(false, total: total);
            foreach (var media in q)
            {
                ja.Add(new JObject()
                {
                    { "Id", media.Id },
                    { "FileName" , media.Filename},
                    { "Description" , media.Description},
                    { "Size" , media.Size},
                    { "Path" , media.Path },
                    { "Type" ,media.Type },
                    { "Uploaded",  media.Uploaded?.ToString(Message.DefaultDatetimeFormat)}
                });
            }
            return ExtJs.WriterJObject(true, total: total, data: ja);
        }

        public async Task<JObject> Delete(int[] id)
        {
            if(id == null) return ExtJs.WriterJObject(false, msg: Message.DeletedNotExist);
            var q = DbContext.Mediae.Where(m => id.Contains(m.Id));
            if (!await q.AnyAsync()) return ExtJs.WriterJObject(false, msg: Message.DeletedNotExist);
            var msgList = new List<string>();
            foreach (var c in q)
            {
                var file = new FileInfo($"{HostintHostingEnvironment.WebRootPath}/upload/{c.Path}/{c.Filename}");
                if (file.Exists) file.Delete();
                DbContext.Mediae.Remove(c);
                msgList.Add(string.Format(MessageListItem, "pointthree", string.Format(Message.Deleted, Message.Media, c.Description)));
            }
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true, msg: string.Format(MessageList, string.Join("", msgList)));
        }

        public JObject DateList()
        {
            var q = from m in DbContext.Mediae
                let year = m.Uploaded.Value.Year
                let month = m.Uploaded.Value.Month
                group m by new { year, month }
                into g
                orderby g.Key.year descending, g.Key.month descending
                select new { year = g.Key.year, month = g.Key.month };
            var ja = new JArray()
            {
                new JObject() { {"Id", "all"}, { "Text", "全部" } },
                new JObject() { {"Id", "today"}, { "Text", "今天" } },
            };
            foreach (var c in q)
            {

                ja.Add(new JObject()
                {
                    {"Id", $"{c.year},{c.month}"}, { "Text", $"{c.year}年{c.month}月"}
                });
            }
            return ExtJs.WriterJObject(true, data: ja);
        }

        public async Task<JObject> Update(int? id, string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 255) return ExtJs.WriterJObject(false, msg: Message.Invalid);
            var q = DbContext.Mediae.SingleOrDefault(m => m.Id == id);
            if (q == null) return ExtJs.WriterJObject(false, msg: string.Format(Message.UpdatedNoExist, Message.Media));
            q.Description = value;
            await DbContext.SaveChangesAsync();
            return ExtJs.WriterJObject(true);
        }


    }
}