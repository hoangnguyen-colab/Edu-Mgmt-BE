using CsvHelper;
using CsvHelper.Configuration;
using Edu_Mgmt_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Edu_Mgmt_BE.Common
{
    public static class Helper
    {
        private static readonly EduManagementContext _db = new EduManagementContext();
        private static readonly string sql_get_role = "SELECT * FROM SystemRole WHERE RoleId IN (SELECT DISTINCT SystemRoleId FROM UserDetail WHERE SystemUserId = @SystemUserId)";
        private static readonly string sql_get_teacher_id = $"SELECT DISTINCT Teacher.* FROM Teacher JOIN UserDetail ON UserDetail.UserId = Teacher.TeacherId JOIN SystemUser ON UserDetail.SystemUserId = @SystemUserId";

        public static string GenerateTeacherID(int index)
        {
            return string.Format("TCR{0}", index.ToString("D4"));
        }

        public static List<Student> GetStudentListCsv(string fileName)
        {
            List<Student> students = new List<Student>();

            var path = $"{Directory.GetCurrentDirectory()}{@"\\Files"}" + "\\" + fileName;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                students = csv.GetRecords<Student>().ToList();
            }

            return students;
        }

        public static string getRole(HttpContext httpContext)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                SystemUser account = jAccount.ToObject<SystemUser>();

                var roles = _db.SystemRole
                    .FromSqlRaw(sql_get_role, new SqlParameter("@SystemUserId", account.SystemUserId))
                    .ToList();

                return roles[0].RoleName;
            }
            return "";
        }

        public static Guid? getTeacherId(HttpContext httpContext)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                SystemUser account = jAccount.ToObject<SystemUser>();

                var teacher = _db.Teacher
                    .FromSqlRaw(sql_get_teacher_id, new SqlParameter("@SystemUserId", account.SystemUserId)).FirstOrDefault();

                return teacher.TeacherId;
            }
            return null;
        }

        public static bool CheckPermission(HttpContext httpContext, string role_code)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                SystemUser account = jAccount.ToObject<SystemUser>();

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
