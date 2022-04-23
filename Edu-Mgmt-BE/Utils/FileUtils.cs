using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.FileSave;
using Edu_Mgmt_BE.Models.CustomModel.Student;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Common
{
    public class FileUtils
    {
        public static Cloudinary cloudinary;
        private readonly string API_KEY = "454226386488799";
        private readonly string API_SECRET = "JhEDW7Tt-8szdykUahR-p3pElFY";
        //private readonly string UPLOAD_PRESET = "bn3jjpdp";
        private readonly string CLOUDINARY_NAME = "dhi8xksch";

        public FileUtils()
        {
            Account account = new Account(CLOUDINARY_NAME, API_KEY, API_SECRET);
            cloudinary = new Cloudinary(account);
        }

        public static bool ImageUpload(string imagePath)
        {
            try
            {
                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(@"D:\Workplace\C#\Edu-Mgmt-BE\Edu-Mgmt-BE\Files\DSC_0249.png")
                //};
                ////var url = cloudinary.Api.UrlImgUp.Transform(new Transformation().Width(100).Height(150).Crop("fill")).BuildUrl("sample.jpg");
                //var uploadResult = cloudinary.Upload(uploadParams); 

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(@"‪C:\Users\Hoag\Downloads\DSC_0249.png")
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public static List<StudentExcel> getStudentListExcel(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<StudentExcel> students = new List<StudentExcel>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
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
                    using (FileStream fileStream = File.Create(filePath))
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

        public static void removeFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
