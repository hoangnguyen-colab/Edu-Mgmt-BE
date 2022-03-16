using CsvHelper;
using CsvHelper.Configuration;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.FileSave;
using Edu_Mgmt_BE.Models.CustomModel.Student;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Edu_Mgmt_BE.Common
{
    public static class Helper
    {
        private static readonly EduManagementContext _db = new EduManagementContext();
        private static readonly string sql_get_role = "SELECT * FROM SystemRole WHERE RoleId IN (SELECT DISTINCT SystemRoleId FROM UserDetail WHERE SystemUserId = @SystemUserId)";
        private static readonly string sql_get_teacher_id = $"SELECT DISTINCT Teacher.* FROM Teacher JOIN UserDetail ON UserDetail.UserId = Teacher.TeacherId JOIN SystemUser ON UserDetail.SystemUserId = @SystemUserId";

        public static List<StudentExcel> getStudentListExcel(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            List<StudentExcel> students = new List<StudentExcel>();
            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    DataRowCollection dt = result.Tables[0].Rows;
                    for (int dataRowCount = 1; dataRowCount < dt.Count; dataRowCount++)
                    {
                        students.Add(new StudentExcel()
                        {
                            StudentName = dt[dataRowCount][1].ToString().Trim(),
                            StudentGender = dt[dataRowCount][2].ToString().Trim(),
                            StudentDob = DateTime.Parse(dt[dataRowCount][3].ToString()).ToString("dd/MM/yyyy"),
                            StudentPhone = Regex.Replace(dt[dataRowCount][4].ToString().Trim(), @"\s", ""),
                        });
                    }
                }
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

        public static Guid? getUserId(HttpContext httpContext)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                SystemUser account = jAccount.ToObject<SystemUser>();

                return account.SystemUserId;
            }
            return null;
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

        public static FileSaveResponse saveFile(HttpContext context, IFormFile file)
        {
            FileSaveResponse res = new FileSaveResponse();
            try
            {
                Dictionary<string, object> account_login = JsonConvert
                    .DeserializeObject<Dictionary<string, object>>(context.User.Identity.Name);
                if (account_login != null && account_login.ContainsKey("account"))
                {
                    JObject jAccount = account_login["account"] as JObject;
                    SystemUser account = jAccount.ToObject<SystemUser>();

                    string defaultPath = $"{Directory.GetCurrentDirectory()}\\Files\\student-import\\{account.UserUsername}\\";

                    if (!Directory.Exists(defaultPath))
                    {
                        Directory.CreateDirectory(defaultPath);
                    }
                    string filePath = defaultPath + file.FileName;
                    using (FileStream fileStream = System.IO.File.Create(filePath))
                    {
                        file.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                    res.filePath = filePath;
                    res.success = true;
                }
                else
                {
                    res.filePath = "";
                    res.success = false;
                }
            }
            catch (Exception)
            {
                res.filePath = "";
                res.success = false;
            }
            return res;
        }
    }
}
