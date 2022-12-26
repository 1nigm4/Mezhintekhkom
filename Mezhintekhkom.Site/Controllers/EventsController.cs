using Mezhintekhkom.Site.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Mezhintekhkom.Site.Controllers
{
    public class EventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(EventJoinViewModel model)
        {
            if (ModelState.IsValid)
            {
                return View();
            }
            return View(model);
        }
    }
}
