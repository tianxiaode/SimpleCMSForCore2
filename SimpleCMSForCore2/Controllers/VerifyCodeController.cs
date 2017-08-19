using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleCMSForCore2.Helper;

namespace SimpleCMSForCore2.Controllers
{
    public class VerifyCodeController : Controller
    {
        // GET: VverifyCode
        public FileContentResult Index()
        {
            var v = new VerifyCode();
            var code = v.CreateVerifyCode();                //取随机码
            HttpContext.Session.SetString("VerifyCode", code)  ;
            v.Padding = 10;
            var bytes = v.CreateImage(code);
            return File(bytes, @"image/jpeg");
        }
    }
}