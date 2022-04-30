using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel;
using Edu_Mgmt_BE.Models.CustomModel.HomeWork;
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

        private const string StudentInClassQuery = "SELECT Student.* FROM Class, ClassDetail, Student WHERE Class.ClassId = @classId AND ClassDetail.ClassId = Class.ClassId AND ClassDetail.StudentId = Student.StudentId";
        private const string query_get_homework_files = "SELECT DISTINCT FileUpload.* FROM HomeWorkFileDetail, FileUpload WHERE HomeWorkFileDetail.HomeWorkId = @homeWorkId AND FileUpload.FileUploadId = HomeWorkFileDetail.FileUploadId";
        private const string query_get_homework_by_class = "SELECT DISTINCT HomeWork.* FROM HomeWorkClassDetail, HomeWork WHERE HomeWork.HomeWorkId = HomeWorkClassDetail.HomeWorkId AND HomeWorkClassDetail.ClassId = @classId";
        private const string query_get_class_by_homework = "SELECT DISTINCT Class.* FROM HomeWorkClassDetail, Class WHERE Class.ClassId = HomeWorkClassDetail.ClassId AND HomeWorkClassDetail.HomeWorkId = @homeWorkId";
        private const string query_get_answer_by_homework = "select Student.*, Answer.AnswerId, ISNULL((SELECT Result.FinalScore FROM Result WHERE Result.AnswerId = Answer.AnswerId), N'_') As FinalScore from Answer, Student WHERE Answer.HomeWorkId = @homeWorkId and Answer.StudentId = Student.StudentId";

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
            [FromQuery] int? record = 10,
            [FromQuery] int? status = 1)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var pagingData = new PagingData();
                List<HomeWork> records = await _db.HomeWork
                    .Where(x => x.HomeWorkStatus == status)
                    .ToListAsync();

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
            [FromQuery] int? record = 10,
            [FromQuery] int? status = 1)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var classResult = await _db.Class.FindAsync(ClassId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                var pagingData = new PagingData();
                var paramId = new SqlParameter("@classId", ClassId);
                List<HomeWork> records = _db.HomeWork
                    .FromSqlRaw(query_get_homework_by_class, paramId)
                    .Where(x => x.HomeWorkStatus == status)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();

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
                foreach (var classId in homeworkReq.ClassList)
                {
                    var class_find = _db.Class.Find(classId);
                    if (class_find == null)
                    {
                        return ErrorHandler.BadRequestResponse(Message.ClassNotFound);
                    }
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
                    HomeWorkContent = homeworkReq.HomeWorkContent,
                    DueDate = DateTimeUtils.UnixTimeStampToDateTime(homeworkReq.DueDate),
                    HomeWorkStatus = HomeWorkStatus.Active,
                    OnlyAssignStudent = homeworkReq.OnlyAssignStudent,
                    RequiredLogin = homeworkReq.RequiredLogin,
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

            var role = Helper.getRole(HttpContext);
            if (role == RoleType.TEACHER)
            {
                var studentList = _db.StudentAnswer
                    .FromSqlRaw(query_get_answer_by_homework, param)
                    .ToList();
                result.Add("studentList", studentList);
            }

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Danh sách học sinh nộp bài
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("detail-answer/{id}")]
        public async Task<ServiceResponse> GetHomeWorkAnswer(Guid? homeWorkId)
        {
            ServiceResponse res = new ServiceResponse();
            var homeWorkResult = await _db.HomeWork.FindAsync(homeWorkId);
            if (homeWorkResult == null)
            {
                return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
            }

            var param = new SqlParameter("@homeWorkid", homeWorkResult.HomeWorkId);
            var studentList = _db.StudentAnswer
                    .FromSqlRaw(query_get_homework_files, param)
                    .ToList();

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("studentList", studentList);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Sửa trạng thái bài tập
        /// </summary>
        /// <param name="classReq"></param>
        /// <returns></returns>
        [HttpPut("edit-status")]
        public async Task<ServiceResponse> EditClassStatus(HomeWorkEditStatus hmreq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var homework_result = await _db.HomeWork.FindAsync(hmreq.HomeWorkId);
                if (homework_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
                }

                homework_result.HomeWorkStatus = hmreq.HomeWorkStatus;
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = homework_result;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Sửa bài tập
        /// </summary>
        /// <param name="homeworkReq"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditHomeWork(Guid? id, HomeWorkReq homeworkReq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (id == null || id == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkEmpty);
                }
                var hm_result = await _db.HomeWork.FindAsync(id);
                if (hm_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
                }

                if (homeworkReq.ClassList is null || homeworkReq.ClassList?.Length == 0)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkClassEmpty);
                }

                foreach (var classId in homeworkReq.ClassList)
                {
                    var class_find = _db.Class.Find(classId);
                    if (class_find == null)
                    {
                        return ErrorHandler.BadRequestResponse(Message.ClassNotFound);
                    }
                }

                if (string.IsNullOrEmpty(homeworkReq.HomeWorkName))
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkNameEmpty);
                }
                if (string.IsNullOrEmpty(homeworkReq.HomeWorkType))
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkTypeEmpty);
                }

                Dictionary<string, object> result = new Dictionary<string, object>();
                hm_result.HomeWorkName = homeworkReq.HomeWorkName.Trim();
                hm_result.HomeWorkType = homeworkReq.HomeWorkType.Trim();
                hm_result.HomeWorkContent = homeworkReq.HomeWorkContent;
                hm_result.DueDate = DateTimeUtils.UnixTimeStampToDateTime(homeworkReq.DueDate);
                hm_result.HomeWorkStatus = homeworkReq.HomeWorkStatus ?? hm_result.HomeWorkStatus;
                hm_result.OnlyAssignStudent = homeworkReq.OnlyAssignStudent;
                hm_result.RequiredLogin = homeworkReq.RequiredLogin;
                result.Add("homeWork", hm_result);

                //remove old files
                var oldFiles = await _db.HomeWorkFileDetail.Where(x => x.HomeWorkId.Equals(id)).ToListAsync();
                _db.HomeWorkFileDetail.RemoveRange(oldFiles);

                if (homeworkReq.FileList?.Length > 0)
                {
                    List<FileUpload> fileList = new List<FileUpload>();
                    List<HomeWorkFileDetail> fileListDetail = new List<HomeWorkFileDetail>();
                    foreach (var file in homeworkReq.FileList)
                    {
                        FileUpload fileItem = new FileUpload()
                        {
                            FileUploadId = Guid.NewGuid(),
                            FileUploadName = file.FileUploadName,
                            FileUploadUrl = file.FileUploadUrl,
                        };

                        fileList.Add(fileItem);
                        fileListDetail.Add(new HomeWorkFileDetail()
                        {
                            FileUploadDetailId = Guid.NewGuid(),
                            HomeWorkId = hm_result.HomeWorkId,
                            FileUploadId = fileItem.FileUploadId,
                        });
                    }

                    _db.FileUpload.AddRange(fileList);
                    _db.HomeWorkFileDetail.AddRange(fileListDetail);
                    result.Add("fileList", fileList);
                    //result.Add("fileDetail", fileListDetail);
                }

                //remove old classes
                var oldClasses = await _db.HomeWorkClassDetail.Where(x => x.HomeWorkId.Equals(id)).ToListAsync();
                _db.HomeWorkClassDetail.RemoveRange(oldClasses);

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
                            HomeWorkId = hm_result.HomeWorkId,
                        });
                        classes.Add(item);
                    }
                }
                result.Add("classes", classes);
                _db.HomeWorkClassDetail.AddRange(classDetailList);

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
        /// Check bài tập
        /// </summary>
        /// <param name="homeworkReq"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("check")]
        public async Task<ServiceResponse> CheckHomeWork(HomeWorkCheck req)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                if (req.ClassId == null || req.ClassId == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
                }
                if (req.HomeWorkId == null || req.HomeWorkId == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkEmpty);
                }

                var classResult = await _db.Class.FindAsync(req.ClassId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                var hwResult = await _db.HomeWork.FindAsync(req.HomeWorkId);
                if (hwResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
                }

                var paramId = new SqlParameter("@classId", req.ClassId);
                List<Student> studentList = _db.Student
                            .FromSqlRaw(StudentInClassQuery, paramId)
                            .OrderByDescending(x => x.StudentName)
                            .ToList();

                Student student = studentList.Where(item => item.StudentPhone.Trim().Equals(req.StudentPhone.Trim())).FirstOrDefault();

                result.Add("student", student);

                if (student != null)
                {
                    var answer_check = await _db.Answer
                        .Where(x => x.ClassId.Equals(req.ClassId)
                        && x.StudentId.Equals(student.StudentId)
                        && x.HomeWorkId.Equals(hwResult.HomeWorkId)
                        )
                        .FirstOrDefaultAsync();

                    if (answer_check != null)
                    {
                        var result_check = await _db.Result
                            .Where(x => x.AnswerId.Equals(answer_check.AnswerId))
                            .FirstOrDefaultAsync();

                        if (result_check != null)
                        {
                            result.Add("result", result_check);
                            res.Success = true;
                            res.Data = result;
                            res.StatusCode = HttpStatusCode.OK;
                            return res;
                        }

                        result.Add("anwer", answer_check);
                        res.Success = true;
                        res.Data = result;
                        res.StatusCode = HttpStatusCode.OK;
                        return res;
                    }
                }
                else
                {
                    if (hwResult.OnlyAssignStudent == true)
                    {
                        var teacher = await _db.Teacher.FindAsync(hwResult.TeacherId);
                        return ErrorHandler.BadRequestResponse(Message.HomeWorkOnlyAssignStudent, teacher);
                    }
                }

                result.Add("class", classResult);

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

    }
}
