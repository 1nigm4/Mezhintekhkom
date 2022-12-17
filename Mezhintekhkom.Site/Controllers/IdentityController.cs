using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class IdentityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Courses(Guid id)
        {
            if (id != default)
                return View("Details");
            return View();
        }
    }
}
