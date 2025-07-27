using Microsoft.AspNetCore.Mvc;

namespace Fuddo.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
