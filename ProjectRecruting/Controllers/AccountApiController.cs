using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;

namespace ProjectRecruting.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        readonly UserManager<ApplicationUser> _userManager=null;
        //readonly ILogger<RegisterModel> _logger=null;

        public AccountApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("Login")]
        public async Task Login([FromForm]string username, [FromForm]string password)
        {
            //var username = Request.Form["username"];
            //var password = Request.Form["password"];

            var identity = await AuthJWT.GetIdentity(username, password, _userManager);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthJWT.ISSUER,
                    audience: AuthJWT.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthJWT.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthJWT.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            // сериализация ответа
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }


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
            // _logger.LogInformation("User created a new account with password.");

            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var callbackUrl = Url.Page(
            //    "/Account/ConfirmEmail",
            //    pageHandler: null,
            //    values: new { userId = user.Id, code = code },
            //    protocol: Request.Scheme);

            //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            //await _signInManager.SignInAsync(user, isPersistent: false);
            //return LocalRedirect(returnUrl);



            var identity = await AuthJWT.GetIdentity(model.Email, model.Password, _userManager);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthJWT.ISSUER,
                    audience: AuthJWT.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthJWT.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthJWT.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
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