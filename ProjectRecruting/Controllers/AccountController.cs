using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProjectRecruting.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return PartialView();
        }

        public IActionResult Register()
        {
            return PartialView();
        }
    }
}