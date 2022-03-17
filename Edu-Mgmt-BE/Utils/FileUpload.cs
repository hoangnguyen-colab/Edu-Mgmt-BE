using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Common
{
    public class FileUpload
    {
        public static Cloudinary cloudinary;
        private readonly string API_KEY = "454226386488799";
        private readonly string API_SECRET = "JhEDW7Tt-8szdykUahR-p3pElFY";
        //private readonly string UPLOAD_PRESET = "bn3jjpdp";
        private readonly string CLOUDINARY_NAME = "dhi8xksch";

        public FileUpload()
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
    }
}
