using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class EventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
