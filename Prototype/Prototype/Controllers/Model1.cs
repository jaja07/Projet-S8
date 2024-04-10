using Microsoft.AspNetCore.Mvc;

namespace Prototype.Controllers
{
    public class Model1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
