using Microsoft.AspNetCore.Mvc;

namespace Prototype.Controllers
{
    public class Model2 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
