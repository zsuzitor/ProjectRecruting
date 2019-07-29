using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRecruting.Models
{
    public class AuthJWT
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "https://localhost:44356/"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 10; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
        //private List<UserShort> people = new List<UserShort>
        //{
        //    new UserShort {Name="admin@gmail.com", IdUser="12345", Password = "admin" },
        //    new UserShort {Name="admin@gmail.com", IdUser="12345", Password = "admin" },
        //};

        public async static Task<ClaimsIdentity> GetIdentity(string username, string password, UserManager<ApplicationUser> userManager)
        {
            //var claims = new List<Claim>
            //    {
            //        new Claim(type:ClaimsIdentity.DefaultNameClaimType,value:"admin@gmail.com"),//,
            //         //new Claim(type:ClaimTypes.Name,value:user.UserName)//,
            //        new Claim(type:ClaimsIdentity.DefaultRoleClaimType, value:"testRole")
            //    };
            //ClaimsIdentity claimsIdentity =
            //new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
            //    ClaimsIdentity.DefaultRoleClaimType);
            //return claimsIdentity;



            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return null;

            var passwordOK = await userManager.CheckPasswordAsync(user, password);
            if (!passwordOK)
                return null;


            var claims = new List<Claim>
                {
                    new Claim(type:ClaimsIdentity.DefaultNameClaimType,value:user.Email),//,
                     //new Claim(type:ClaimTypes.Name,value:user.UserName)//,
                    new Claim(type:ClaimsIdentity.DefaultRoleClaimType, value:"testRole")
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;


            // если пользователя не найдено
            //return null;
        }
    }
}
