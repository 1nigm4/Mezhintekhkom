using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
