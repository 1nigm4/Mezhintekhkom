using Mezhintekhkom.Site.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Mezhintekhkom.Site.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
            return View();
		}

		public IActionResult Teachers()
		{
			return View();
		}

		public IActionResult Information()
		{
			return View();
		}

		public IActionResult Contacts()
		{
			return View();
		}
	}
}