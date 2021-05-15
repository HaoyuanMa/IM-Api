using IdentityModel;
using IM_Api.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IM_Api.Tools
{
    public static class JwtTools
    {
        private static string issuer = "mahaoyuan";
        private static string audience = null;
        private static SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SKDJF5D4SG65WE8SD56G4A8SD5FWE5"));


        public static string GenerateJwtToken(string email,string client)
        {
            // 1. 设置加密算法
            string algorithm = SecurityAlgorithms.HmacSha256;

            // 2. 生成签名证书，注意密钥长度至少为16位，否则会报错
           

            SigningCredentials signing = new SigningCredentials(key, algorithm);

            // 3. 构造Claims(任意添加几个值，实际项目根据需要来设置要传递的信息)
           

            // 4. 生成令牌
            
            DateTime notBefore = DateTime.Now;
            DateTime expires;
            if(client == "web")
            {
                audience = "web";
                expires = DateTime.Now.AddHours(6);
            }
            else
            {
                audience = "api";
                expires = DateTime.Now.AddDays(45);
            }

            var claimsidentity = new ClaimsIdentity(new Claim[] {
                    new Claim(JwtClaimTypes.Issuer, issuer),
                    new Claim(JwtClaimTypes.Name, email),
                    new Claim(JwtClaimTypes.NotBefore, notBefore.ToString()),
                    new Claim(JwtClaimTypes.Audience, audience),
                }, nameType: "name", authenticationType: null, roleType: null);
                

            var tokendescripor = new SecurityTokenDescriptor
            {
                Subject = claimsidentity,
                Expires = expires,
                SigningCredentials = signing
            };

            SecurityToken jwtToken = new JwtSecurityTokenHandler().CreateToken(tokendescripor);
                //new JwtSecurityToken(issuer, audience, cla, notBefore, expires, signing);
            
            // 5. 将令牌实例转换成字符串
            string strToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return strToken;
                
        }

        public static bool DecodeJwtToken(string token, out string email)
        {
            var JwtManager = new JwtSecurityTokenHandler();
            
            SecurityToken validatedToken;

            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                IssuerSigningKey = key
            };

            ClaimsPrincipal claimsPrincipal;

            try
            {
                //claimsPrincipal = 
                JwtManager.ValidateToken(token, validationParameters, out validatedToken);
                //email= claimsPrincipal.Claims.FirstOrDefault().Value;
                email = (validatedToken as JwtSecurityToken).Claims.FirstOrDefault().Value;
                return true;
            }
            catch (SecurityTokenException)
            {
                email = null;
                //throw;
                return false;
            }
            catch (Exception e)
            {
                email = null;
                //throw;
                //log(e.ToString()); //something else happened
                return false;
            }
        }

    }
}
