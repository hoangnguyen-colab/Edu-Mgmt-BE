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

namespace Edu_Mgmt_BE
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string key;
        public JwtAuthenticationManager(string key)
        {
            this.key = key;
        }

        public ServiceResponse LoginAuthenticate(EduManagementContext _db, string username, string password)
        {
            ServiceResponse res = new ServiceResponse();
            string passwordMD5 = Helper.EncodeMD5(password);
            //var accountResult = _db.Accounts.Where(_ => _.Username == username && _.Password == passwordMD5).FirstOrDefault();
            //if (accountResult == null)
            //{
            //    res.Message = "Thông tin đăng nhập không chính xác!";
            //    res.Success = true;
            //    res.Data = null;
            //    res.ErrorCode = 404;
            //    return res;
            //}
            //accountResult.LastLogin = DateTime.Now;
            //_db.Entry(accountResult).State = EntityState.Modified;
            //string sql_get_role = $"select * from role where role_id in (select distinct role_id from account_role where account_id = @account_id)";
            //var roles = _db.Roles.FromSqlRaw(sql_get_role, new SqlParameter("@account_id", accountResult.AccountId)).ToList();

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("account", username);
            result.Add("roles", passwordMD5);
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
            result.Add("token", tokenHandler.WriteToken(token));
            res.Success = true;
            res.Data = result;
            return res;
        }
    }
}
