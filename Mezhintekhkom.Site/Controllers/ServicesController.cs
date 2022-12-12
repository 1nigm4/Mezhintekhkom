using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LaborProtection()
        {
            return View();
        }
    }
}
