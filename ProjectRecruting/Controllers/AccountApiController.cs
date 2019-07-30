using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        readonly UserManager<ApplicationUser> _userManager=null;
        readonly ApplicationDbContext _db= null;
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
            //var username = Request.Form["username"];
            //var password = Request.Form["password"];
            var user = await ApplicationUser.LoginGet(_userManager, username, password);
            if (user == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }
            var identity =  AuthJWT.GetIdentity(user);
            //if (identity == null)
            //{
            //    Response.StatusCode = 400;
            //    await Response.WriteAsync("Invalid username or password.");
            //    return;
            //}
            var encodedJwt = AuthJWT.GenerateMainToken(identity);
            var encodedRefJwt = AuthJWT.GenerateRefreshToken(10);
            await user.SetRefreshToken(_db,encodedRefJwt);
            //var now = DateTime.UtcNow;
            //// создаем JWT-токен
            //var jwt = new JwtSecurityToken(
            //        issuer: AuthJWT.ISSUER,
            //        audience: AuthJWT.AUDIENCE,
            //        notBefore: now,
            //        claims: identity.Claims,
            //        expires: now.Add(TimeSpan.FromMinutes(AuthJWT.LIFETIME)),
            //        signingCredentials: new SigningCredentials(AuthJWT.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            //var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                refresh_token= encodedRefJwt,
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
                return ;
            }

                var username = Request.Form["username"];
            //var password = Request.Form["password"];


            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                Response.StatusCode = 404;
                return ;
            }
            

            var identity =  AuthJWT.GetIdentity(user);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var encodedJwt = AuthJWT.GenerateMainToken(identity);
            var encodedRefJwt = AuthJWT.GenerateRefreshToken(10);
            await user.SetRefreshToken(_db, encodedRefJwt);

            //var now = DateTime.UtcNow;
            //// создаем JWT-токен
            //var jwt = new JwtSecurityToken(
            //        issuer: AuthJWT.ISSUER,
            //        audience: AuthJWT.AUDIENCE,
            //        notBefore: now,
            //        claims: identity.Claims,
            //        expires: now.Add(TimeSpan.FromMinutes(AuthJWT.LIFETIME)),
            //        signingCredentials: new SigningCredentials(AuthJWT.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            //var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

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


        [HttpGet("Home")]
        public  string Home()//, string confirmPassword
        {
            var g = 10;
            return "12344";
        }

        }
}