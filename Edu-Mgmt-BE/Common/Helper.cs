using Edu_Mgmt_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Edu_Mgmt_BE.Common
{
    public static class Helper
    {
        private static readonly EduManagementContext _db = new EduManagementContext();

        /// <summary>
        /// Check quyền
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="_cache"></param>
        /// <param name="ip"></param>
        /// <param name="acction"></param>
        /// <returns></returns>
        public static bool CheckPermission(HttpContext httpContext, string role_code)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                SystemUser account = jAccount.ToObject<SystemUser>();

                string sql_get_role = $"select * from SystemRole where RoleId in (select distinct SystemRoleId from UserDetail where SystemUserId = @SystemUserId)";
                var roles = _db.SystemRole
                    .FromSqlRaw(sql_get_role, new SqlParameter("@SystemUserId", account.SystemUserId))
                    .ToList();

                for (int i = 0; i < roles.Count(); i++)
                {
                    if (roles[i].RoleName == role_code)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string EncodeMD5(string str)
        {
            string result = "";
            if (str != null)
            {
                MD5 md = MD5.Create();
                byte[] bytePass = Encoding.ASCII.GetBytes(str);
                byte[] byteResult = md.ComputeHash(bytePass);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < byteResult.Length; i++)
                {
                    sb.Append(byteResult[i].ToString("X2"));
                }
                result = sb.ToString();
            }
            return result;
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> input, string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return input;

            int i = 0;
            foreach (string propname in queryString.Split(','))
            {
                var subContent = propname.Split(' ');
                if (subContent[1].Trim().ToLower() == "asc")
                {
                    if (i == 0)
                        input = input.OrderBy(x => GetPropertyValue(x, subContent[0].Trim()));
                    else
                        input = ((IOrderedEnumerable<T>)input).ThenBy(x => GetPropertyValue(x, subContent[0].Trim()));
                }
                else
                {
                    if (i == 0)
                        input = input.OrderByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
                    else
                        input = ((IOrderedEnumerable<T>)input).ThenByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
                }
                i++;
            }

            return input;
        }
       
        private static object GetPropertyValue(object obj, string property)
        {
            System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
            return propertyInfo.GetValue(obj, null);
        }
    }
}
