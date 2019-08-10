using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectRecruting.Data;
using ProjectRecruting.Models;
using ProjectRecruting.Models.Domain;
using ProjectRecruting.Models.services;

namespace ProjectRecruting.Controllers
{
    [Produces("application/json")]
    [Route("api/account")]
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


        /// <summary>
        /// логин
        /// </summary>
        /// <param name="username">логин(почта)</param>
        /// <param name="password">пароль</param>
        /// <returns>
        /// {
        ///access_token,
        ///refresh_token,
        ///username
        ///}
        /// </returns>
        /// <response code="200"></response>
        /// <response code="400">пользователь не найден</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task Login([FromForm]string username, [FromForm]string password)
        {
            //try
            //{

            
            var user = await ApplicationUser.LoginGet(_userManager, username, password);
            //}
            //catch(Exception e)
            //{
            //    var asd = 1;
            //}
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


        /// <summary>
        /// регистрация
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// {
        ///access_token,
        ///refresh_token,
        ///username
        ///}
        /// </returns>
        ///<response code="200"></response>
        ///<response code="400">переданы невалидные данные</response>
        ///<response code="404">ошибка при создании пользователя</response>
        ///<response code="500">ошибка получения токена</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [HttpPost("register")]
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
                Response.StatusCode = 500;
                // await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var encodedJwt = AuthJWT.GenerateMainToken(identity);
            var encodedRefJwt = AuthJWT.GenerateRefreshToken();
            await user.SetRefreshToken(_db, encodedRefJwt);


            //--------этот блок для подтверждения почты
            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var callbackUrl = Url.Action(
            //    "ConfirmEmail",
            //    "Account",
            //    new { userId = user.Id, code = code },
            //    protocol: HttpContext.Request.Scheme);
            //EmailService emailService = new EmailService();
            //await emailService.SendEmailAsync(model.Email, "Confirm your account",
            //    $"Подтвердите регистрацию, перейдя по ссылке: <a href='{callbackUrl}'>link</a>");

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


        /// <summary>
        /// обновление токена
        /// </summary>
        /// <param name="refreshToken">refresh token</param>
        /// <returns>
        /// {
        ///access_token,
        ///refresh_token
        ///}
        /// </returns>
        /// <response code="401">ошибка дешифрации токена, просрочен, изменен, не передан</response>
        /// <response code="404">--ошибка обновления токена</response>
        [ProducesResponseType(401)]
        //[ProducesResponseType(404)]
        [HttpPost("refresh-token")]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [HttpGet("confirm-email")]
        //[AllowAnonymous]
        public async Task ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                Response.StatusCode = 400;
                return;
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Response.StatusCode = 404;
                return;
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                Response.StatusCode = 200;
            else
                Response.StatusCode = 500;
            return;
        }

        /// <summary>
        /// удаляет refresh token из бд
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="refreshToken">refresh token</param>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<bool> LogOut([FromForm]string userId, [FromForm] string refreshToken)
        {
            return await AuthJWT.DeleteRefreshTokenFromDb(_db, userId, refreshToken);

        }

        //[HttpGet("Home")]
        //public string Home()//, string confirmPassword
        //{


        //    return "12344";
        //}

    }
}