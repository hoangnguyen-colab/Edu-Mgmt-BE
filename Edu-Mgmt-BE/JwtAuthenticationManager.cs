using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using System.Threading.Tasks;
using System.Net;

namespace Edu_Mgmt_BE
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string key;
        public JwtAuthenticationManager(string key)
        {
            this.key = key;
        }

        public async Task<ServiceResponse> LoginAuthenticate(EduManagementContext _db, string username, string password)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                string passwordMD5 = Helper.EncodeMD5(password);
                var accountResult = await _db.SystemUser
                    .Where(_ => _.UserUsername == username && _.UserPassword == passwordMD5)
                    .FirstOrDefaultAsync();
                if (accountResult == null)
                {
                    res.Message = Constants.Message.LoginIncorrect;
                    res.Success = true;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.BadRequest;
                    return res;
                }
                accountResult.UserPassword = null;
                string sql_get_role = $"select * from SystemRole where RoleId in (select distinct SystemRoleId from UserDetail where SystemUserId = @SystemUserId)";
                var roles = await _db.SystemRole
                    .FromSqlRaw(sql_get_role, new SqlParameter("@SystemUserId", accountResult.SystemUserId))
                    .ToListAsync();

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("account", accountResult);
                result.Add("roles", roles);
                result.Add("token", EncodeJWTToken(result));
                res.Success = true;
                res.Data = result;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception)
            {
                res.Message = Constants.Message.ErrorMsg;
                res.Success = true;
                res.Data = null;
                res.StatusCode = HttpStatusCode.InternalServerError;
                return res;
            }
            return res;
        }

        private string EncodeJWTToken(object result)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(key);
            var tokenDesciptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,JsonConvert.SerializeObject(result))
                }),
                Expires = DateTime.UtcNow.AddDays(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDesciptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
