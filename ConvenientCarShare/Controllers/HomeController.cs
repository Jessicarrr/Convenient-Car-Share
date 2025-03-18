using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ConvenientCarShare.Models;
using ConvenientCarShare.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace ConvenientCarShare.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult About()
        {
            ViewData["Message"] = "Convenient Car Share";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contact";

            return View();
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
