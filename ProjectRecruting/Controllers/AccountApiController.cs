using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        readonly UserManager<ApplicationUser> _userManager = null;
        readonly ApplicationDbContext _db = null;
        //readonly ILogger<RegisterModel> _logger=null;

        public AccountApiController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task Login([FromForm]string username, [FromForm]string password)
        {

            var user = await ApplicationUser.LoginGet(_userManager, username, password);
            if (user == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }
            var identity = AuthJWT.GetIdentity(user);

            var encodedJwt = AuthJWT.GenerateMainToken(identity);
            var encodedRefJwt = AuthJWT.GenerateRefreshToken();
            await user.SetRefreshToken(_db, encodedRefJwt);
            var response = new
            {
                access_token = encodedJwt,
                refresh_token = encodedRefJwt,
                username = identity.Name
            };

            // сериализация ответа
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task Register([FromForm]RegisterModel model)//, string confirmPassword
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return;
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                Response.StatusCode = 404;
                return;
            }


            var identity = AuthJWT.GetIdentity(user);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var encodedJwt = AuthJWT.GenerateMainToken(identity);
            var encodedRefJwt = AuthJWT.GenerateRefreshToken();
            await user.SetRefreshToken(_db, encodedRefJwt);

            var response = new
            {
                access_token = encodedJwt,
                refresh_token = encodedRefJwt,
                username = identity.Name
            };

            // сериализация ответа
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        [HttpPost("RefreshToken")]
        public async Task RefreshToken([FromForm]string refreshToken)//, string confirmPassword
        {
            string userId = AuthJWT.GetCurrentId(HttpContext, out int status);

            if ((status != 0 && status != 1) || userId == null)
            {
                Response.StatusCode = 401;
                return;
            }
            var tokens = await AuthJWT.Refresh(_db, userId, refreshToken);
            if (tokens == null)
            {
                Response.StatusCode = 401;
                return;
            }
            var response = new
            {
                access_token = tokens.Item1,
                refresh_token = tokens.Item2
            };

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));


            //return status.ToString();

        }


        //[HttpGet("Home")]
        //public string Home()//, string confirmPassword
        //{


        //    return "12344";
        //}

    }
}