using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers
{
    public class SessionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}