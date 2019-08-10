using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectRecruting.Models;

namespace ProjectRecruting.Controllers
{
    [Route("home")]
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet("index")]
        [HttpGet("")]
        public IActionResult Index()
        {

            return View();
        }

        //страница с выбором раздела
        [HttpGet("main-page")]
        public IActionResult MainPage()
        {

            return PartialView();
        }

        [HttpGet("companys-page")]
        public IActionResult CompanysPage()
        {

            return PartialView();
        }

        [HttpGet("projects-page")]
        public IActionResult ProjectsPage()
        {

            return PartialView();
        }

        [HttpGet("create-company")]
        public IActionResult CreateCompany()
        {

            return PartialView();
        }

        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
