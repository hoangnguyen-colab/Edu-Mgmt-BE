﻿using Edu_Mgmt_BE.Common;
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

        private const string query_get_homework_files = "SELECT DISTINCT FileUpload.* FROM HomeWork, HomeWorkFileDetail, FileUpload WHERE HomeWork.HomeWorkId = @homeWorkId AND HomeWork.HomeWorkId = HomeWorkFileDetail.HomeWorkId AND FileUpload.FileUploadId = HomeWorkFileDetail.FileUploadId";

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
        public async Task<ServiceResponse> GetClassByPagingAndSearch(
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
                    DueDate = DateTimeUtils.UnixTimeStampToDateTime(homeworkReq.DueDate),
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

                _db.HomeWork.AddRange(homeWork);
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

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("homeWork", homeWorkResult);
            result.Add("files", filesRecords);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

    }
}
