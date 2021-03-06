using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Controllers
{
    [Authorize]
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly EduManagementContext _db;

        public FileController(EduManagementContext context)
        {
            _db = context;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<ServiceResponse> UploadFile(IFormFile file)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (file.Length > 0)
                {
                    //string fileSave = Helper.saveFile(file);
                    //res.Data = FileUpload.ImageUpload("D:\\Workplace\\C#\\Edu-Mgmt-BE\\Edu-Mgmt-BE\\Files\\DSC_0249.png");
                }
                else
                {
                    res.Data = null;
                }

                res.Success = true;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// View file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> ViewFile(Guid id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {

                var file = await _db.FileUpload.FindAsync(id);

                if (file == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.FileNotFound);
                }

                res.Data = file;
                res.Success = true;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }


    }
}
