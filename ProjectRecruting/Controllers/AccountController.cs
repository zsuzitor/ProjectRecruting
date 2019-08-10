using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProjectRecruting.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            return PartialView();
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return PartialView();
        }
    }
}