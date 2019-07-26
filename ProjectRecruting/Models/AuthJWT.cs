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
        public const string AUDIENCE = "http://localhost:51884/"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 1; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }


        public async static Task<ClaimsIdentity> GetIdentity(string username, string password, UserManager<ApplicationUser> userManager)
        {
            
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return null;

            var passwordOK = await userManager.CheckPasswordAsync(user, password);
            if (!passwordOK)
                return null;

            if (user!= null)
            {
                var claims = new List<Claim>
                {
                    new Claim(type:ClaimsIdentity.DefaultNameClaimType,value:user.UserName)//,
                    //new Claim(type:ClaimsIdentity.DefaultRoleClaimType, value:user.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }
    }
}
