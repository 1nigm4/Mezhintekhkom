using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
