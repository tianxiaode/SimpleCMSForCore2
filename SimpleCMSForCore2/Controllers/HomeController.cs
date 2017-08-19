using Microsoft.AspNetCore.Mvc;

namespace SimpleCMSForCore2.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public IActionResult Index()
        {
            return View();
        }

    }
}