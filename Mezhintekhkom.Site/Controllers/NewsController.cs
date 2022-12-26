using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index(string type = null)
        {
            ViewBag.Type = type;
            return View();
        }
    }
}
