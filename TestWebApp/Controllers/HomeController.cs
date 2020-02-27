using System;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using SharedModels;
using TestWebApp.Models;

namespace TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        private ISessionStore _sessionStore;

        public HomeController(ISessionStore sessionStore)
        {
            _sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Session");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
