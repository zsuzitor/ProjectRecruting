using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using ProjectRecruting.Data;
using ProjectRecruting.Models.Domain;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRecruting.Models.services
{
    public class AuthJWT
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "https://localhost:44356/"; // потребитель токена
        public const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 10; // время жизни токена - 1 минута
        public const int LengthRefreshToken = 10;
        //public const int JwtExpireDays = 10; 
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }


        public async static Task<ClaimsIdentity> GetIdentity(string username, string password, UserManager<ApplicationUser> userManager)
        {
            var user = await ApplicationUser.LoginGet(userManager, username, password);

            return AuthJWT.GetIdentity(user);

        }

        public static ClaimsIdentity GetIdentity(ApplicationUser user)
        {

            if (user == null)
                return null;

            var claims = new List<Claim>
                {
                    new Claim(type:ClaimsIdentity.DefaultNameClaimType,value:user.Id),//,Email
                     //new Claim(type:ClaimTypes.Name,value:user.UserName)//,
                    new Claim(type:ClaimsIdentity.DefaultRoleClaimType, value:"testRole")
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;

        }


        public static string GenerateRefreshToken()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, AuthJWT.LengthRefreshToken)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateMainToken(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthJWT.ISSUER,
                    audience: AuthJWT.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthJWT.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthJWT.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


        //кортеж item1-основной токен item2-рефлеш
        public async static Task<Tuple<string, string>> Refresh(ApplicationDbContext db, string userId, string refreshToken)
        {
            int hashToken = refreshToken.GetHashCode();
            var user = await db.Users.FirstOrDefaultAsync(x1 => x1.Id == userId && x1.RefreshTokenHash == hashToken);
            if (user == null)
                return null;
            string token = AuthJWT.GenerateRefreshToken();
            await user.SetRefreshToken(db, token);

            return new Tuple<string, string>(AuthJWT.GenerateMainToken(AuthJWT.GetIdentity(user)), token);
        }

        //кортеж item1-основной токен item2-рефлеш
        public async static Task<Tuple<string, string>> Refresh(ApplicationDbContext db, UserManager<ApplicationUser> userManager, string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return null;

            var passwordOK = await userManager.CheckPasswordAsync(user, password);
            if (!passwordOK)
                return null;

            string refToken = AuthJWT.GenerateRefreshToken();
            await user.SetRefreshToken(db, refToken);

            return new Tuple<string, string>(AuthJWT.GenerateMainToken(AuthJWT.GetIdentity(user)), refToken);
        }

        public async static Task<bool> DeleteRefreshTokenFromDb(ApplicationDbContext db, string userId, string refreshToken)
        {
            int hashToken = refreshToken.GetHashCode();
            var user = await db.Users.FirstOrDefaultAsync(x1 => x1.Id == userId && x1.RefreshTokenHash == hashToken);
            if (user == null)
                return false;
            user.RefreshTokenHash = null;
            await db.SaveChangesAsync();
            return true;

        }

        public static ClaimsPrincipal DecodeToken(StringValues authorizationToken, out SecurityToken tokenSecure)
        {
            //tokenSecure = null;
            var key = Encoding.ASCII.GetBytes(AuthJWT.KEY);
            var handler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            return handler.ValidateToken(authorizationToken, validations, out tokenSecure);
        }


        public static string GetCurrentId(HttpContext context, out int status)
        {
            context.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
            status = 0;
            try
            {
                var claims = AuthJWT.DecodeToken(authorizationToken, out SecurityToken token);
                return claims.Identity.Name;
            }
            catch (SecurityTokenExpiredException)//просрочен
            {
                status = 1;
                var token = new JwtSecurityTokenHandler().ReadJwtToken(authorizationToken);
                return token.Claims.FirstOrDefault(x1 => x1.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            }
            catch (SecurityTokenValidationException)//изменен извне(\поломан\недопустим)
            {
                status = 2;
            }
            catch (Exception)//все остальное, должно быть в конце
            {
                status = 3;
            }
            return null;
        }
    }
}
