using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel;
using Edu_Mgmt_BE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Controllers
{
    [Authorize]
    [Route("api/home-work")]
    [ApiController]
    public class HomeWorkController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        private const string query_get_homework_files = "SELECT DISTINCT FileUpload.* FROM HomeWorkFileDetail, FileUpload WHERE HomeWorkFileDetail.HomeWorkId = @homeWorkId AND FileUpload.FileUploadId = HomeWorkFileDetail.FileUploadId";
        private const string query_get_homework_by_class = "SELECT DISTINCT HomeWork.* FROM HomeWorkClassDetail, HomeWork WHERE HomeWork.HomeWorkId = HomeWorkClassDetail.HomeWorkId AND HomeWorkClassDetail.ClassId = @classId";
        private const string query_get_class_by_homework = "SELECT DISTINCT Class.* FROM HomeWorkClassDetail, Class WHERE Class.ClassId = HomeWorkClassDetail.ClassId AND HomeWorkClassDetail.HomeWorkId = @homeWorkId";

        public HomeWorkController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách bài tập có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/home-work?page=2&record=10&search=21
        [HttpGet]
        public async Task<ServiceResponse> GetHomeWorkPagingAndSearch(
            [FromQuery] string search,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var pagingData = new PagingData();
                List<HomeWork> records = await _db.HomeWork.ToListAsync();

                string role = Helper.getRole(HttpContext);

                res.Success = true;
                res.Data = new PagingData()
                {
                    TotalRecord = records.Count(),
                    TotalPage = Convert.ToInt32(Math.Ceiling((decimal)records.Count() / (decimal)record.Value)),
                    Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(),
                };
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }


        /// <summary>
        /// Lấy danh sách bài tập theo lớp có phân trang
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/home-work?page=2&record=10&search=21
        [HttpGet("by-class/{ClassId}")]
        public async Task<ServiceResponse> GetClassByClassPagingAndSearch(
            Guid ClassId,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var class_result = _db.Class.Find(ClassId);
                var classResult = await _db.Class.FindAsync(ClassId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                var pagingData = new PagingData();
                List<HomeWork> records = await _db.HomeWork.ToListAsync(); 
                var paramId = new SqlParameter("@classId", ClassId);
                records = _db.HomeWork
                    .FromSqlRaw(query_get_homework_by_class, paramId)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();


                string role = Helper.getRole(HttpContext);

                res.Success = true;
                res.Data = new PagingData()
                {
                    TotalRecord = records.Count(),
                    TotalPage = Convert.ToInt32(Math.Ceiling((decimal)records.Count() / (decimal)record.Value)),
                    Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(),
                };
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Thêm bài tập
        /// </summary>
        /// <param name="homeworkReq"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<ServiceResponse> AddHomeWork(HomeWorkReq homeworkReq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (homeworkReq.ClassList is null || homeworkReq.ClassList?.Length == 0)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkClassEmpty);
                }
                var teacherId = Helper.getTeacherId(HttpContext);
                if (string.IsNullOrEmpty(homeworkReq.HomeWorkName))
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkNameEmpty);
                }
                if (string.IsNullOrEmpty(homeworkReq.HomeWorkType))
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkTypeEmpty);
                }

                Dictionary<string, object> result = new Dictionary<string, object>();
                HomeWork homeWork = new HomeWork()
                {
                    HomeWorkId = Guid.NewGuid(),
                    HomeWorkName = homeworkReq.HomeWorkName.Trim(),
                    HomeWorkType = homeworkReq.HomeWorkType.Trim(),
                    HomeWorkDescribe = homeworkReq.HomeWorkDescribe,
                    DueDate = DateTimeUtils.UnixTimeStampToDateTime(homeworkReq.DueDate),
                    HomeWorkStatus = HomeWorkStatus.Active,
                    CreatedDate = DateTime.Now,
                    TeacherId = string.IsNullOrEmpty(teacherId.ToString()) ? null : teacherId,
                };
                result.Add("homeWork", homeWork);

                if (homeworkReq.FileList?.Length > 0)
                {
                    List<Models.FileUpload> fileList = new List<Models.FileUpload>();
                    List<Models.HomeWorkFileDetail> fileListDetail = new List<Models.HomeWorkFileDetail>();
                    foreach (var file in homeworkReq.FileList)
                    {
                        Models.FileUpload fileItem = new Models.FileUpload()
                        {
                            FileUploadId = Guid.NewGuid(),
                            FileUploadName = file.FileUploadName,
                            FileUploadUrl = file.FileUploadUrl,
                        };

                        fileList.Add(fileItem);
                        fileListDetail.Add(new HomeWorkFileDetail()
                        {
                            FileUploadDetailId = Guid.NewGuid(),
                            HomeWorkId = homeWork.HomeWorkId,
                            FileUploadId = fileItem.FileUploadId,
                        });
                    }
                    _db.FileUpload.AddRange(fileList);
                    _db.HomeWorkFileDetail.AddRange(fileListDetail);
                    result.Add("fileList", fileList);
                    //result.Add("fileDetail", fileListDetail);
                }

                List<HomeWorkClassDetail> classDetailList = new List<HomeWorkClassDetail>();
                List<Class> classes = new List<Class>();
                foreach (var classId in homeworkReq.ClassList)
                {
                    Class item = await _db.Class.FindAsync(classId);
                    if (item != null)
                    {
                        classDetailList.Add(new HomeWorkClassDetail()
                        {
                            ClassId = item.ClassId,
                            HomeWorkId = homeWork.HomeWorkId,
                        });
                        classes.Add(item);
                    }
                }
                result.Add("classes", classes);
                _db.HomeWorkClassDetail.AddRange(classDetailList);

                _db.HomeWork.Add(homeWork);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = result;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Chi tiết bài tập
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetHomeWorkDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var homeWorkResult = await _db.HomeWork.FindAsync(id);
            if (homeWorkResult == null)
            {
                return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
            }

            var param = new SqlParameter("@homeWorkid", homeWorkResult.HomeWorkId);
            var filesRecords = _db.FileUpload
                .FromSqlRaw(query_get_homework_files, param)
                .OrderByDescending(x => x.FileUploadName)
                .ToList();

            var classRecords = _db.Class
                .FromSqlRaw(query_get_class_by_homework, param)
                .OrderByDescending(x => x.ClassName)
                .ToList();

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("homeWork", homeWorkResult);
            result.Add("files", filesRecords);
            result.Add("class", classRecords);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

    }
}
