using CsvHelper;
using CsvHelper.Configuration;
using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Student;
using Edu_Mgmt_BE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Controllers
{
    [Authorize]
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private static IWebHostEnvironment _environment;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        public StudentController(EduManagementContext context,
            IJwtAuthenticationManager jwtAuthenticationManager,
            IWebHostEnvironment environment
            )
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _environment = environment;
        }

        /// <summary>
        /// Lấy danh sách học sinh có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/accounts?page=2&record=10&search=admin
        [HttpGet]
        public async Task<ServiceResponse> GetStudentsByPagingAndSearch(
            [FromQuery] string search,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (!Helper.CheckPermission(HttpContext, "admin") &&
                    !Helper.CheckPermission(HttpContext, "teacher"))
                {
                    res.Success = false;
                    res.Message = Message.NotAuthorize;
                    res.ErrorCode = 403;
                    res.StatusCode = HttpStatusCode.Unauthorized;
                    return res;
                }
                var pagingData = new PagingData();
                List<Student> records = new List<Student>();

                if (search != null && search.Trim() != "")
                {
                    string sql_get_account = "select * from Student where CHARINDEX(@txtSeach, StudentName) > 0";
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.Student
                        .FromSqlRaw(sql_get_account, param)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();
                }
                else
                {
                    records = await _db.Student
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync();
                }

                pagingData.TotalRecord = records.Count();
                pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
                pagingData.Data = records
                    .Skip((page.Value - 1) * record.Value)
                    .Take(record.Value)
                    .ToList();

                res.Success = true;
                res.Data = pagingData;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception)
            {
                res.Message = Message.ErrorMsg;
                res.Success = false;
                res.ErrorCode = 500;
                res.StatusCode = HttpStatusCode.InternalServerError;
            }
            return res;
        }

        /// <summary>
        /// Lấy chi tiết thông tin học sinh
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetStudent(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var student = await _db.Student.FindAsync(id);
            if (student == null)
            {
                res.Data = null;
                res.Message = Message.StudentNotFound;
                res.StatusCode = HttpStatusCode.NotFound;
                return res;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("student", student);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Thêm học sinh
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> AddStudent(AddStudentRequest studentReq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 401;
                res.StatusCode = HttpStatusCode.Unauthorized;
                return res;
            }
            try
            {
                if (string.IsNullOrEmpty(studentReq.StudentName))
                {
                    res.Message = Message.StudentNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                if (string.IsNullOrEmpty(studentReq.StudentPhone))
                {
                    res.Message = Message.StudentPhoneEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                ClassDetail classDetail = null;
                if (!string.IsNullOrEmpty(studentReq.ClassId.ToString()))
                {
                    var classResult = await _db.Class.FindAsync(studentReq.ClassId);
                    if (classResult == null)
                    {
                        res.Message = Message.ClassNotFound;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        res.StatusCode = HttpStatusCode.NotFound;

                        return res;
                    }
                    var yearResult = await _db.SchoolYear.FindAsync(studentReq.SchoolYearId);
                    if (classResult == null)
                    {
                        res.Message = Message.SchoolYearNotFound;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        res.StatusCode = HttpStatusCode.NotFound;

                        return res;
                    }

                    classDetail = await _db.ClassDetail
                        .Where(item => studentReq.ClassId.Equals(item.ClassId)
                        && studentReq.SchoolYearId.Equals(item.SchoolYearId))
                        .FirstOrDefaultAsync();
                    if (classDetail == null)
                    {
                        classDetail = new ClassDetail()
                        {
                            ClassDetailId = Guid.NewGuid(),
                            SchoolYearId = studentReq.SchoolYearId.Value,
                        };
                        //_db.ClassDetail.Add(classDetail);
                    }
                }
                Student student = new Student()
                {
                    ShowStudentId = Helper.GenerateStudentID(_db.Student.Count() + 1),
                    StudentId = Guid.NewGuid(),
                    StudentName = studentReq.StudentName.Trim(),
                    StudentGender = studentReq.StudentGender?.Trim() ?? "",
                    StudentAddress = studentReq.StudentAddress?.Trim() ?? "",
                    StudentImage = studentReq.StudentImage?.Trim() ?? "",
                    StudentDOB = studentReq.StudentDOB?.Trim() ?? "",
                    StudentDescription = studentReq.StudentDescription?.Trim() ?? "",
                    StudentPhone = studentReq.StudentPhone.Trim(),
                };
                _db.Student.Add(student);

                SystemUser sysUser = new SystemUser
                {
                    SystemUserId = Guid.NewGuid(),
                    Username = student.StudentName,
                    UserUsername = student.ShowStudentId.ToLower(),
                    UserPassword = Helper.EncodeMD5(student.ShowStudentId.ToLower()),
                };
                _db.SystemUser.Add(sysUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = student.StudentId,
                    SystemRoleId = 3,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(3);

                if (classDetail != null)
                {
                    classDetail.StudentId = student.StudentId;
                }

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("student", student);
                result.Add("studentAccount", sysUser);
                result.Add("role", role);
                result.Add("classDetail", classDetail);
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
        /// Nhập file học sinh
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost("import")]
        public async Task<ServiceResponse> ImportStudent(IFormFile file)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 401;
                res.StatusCode = HttpStatusCode.Unauthorized;
                return res;
            }
            try
            {
                if (file.Length > 0)
                {
                    var fileRes = Helper.saveFile(HttpContext, file);
                    if (fileRes.success)
                    {
                        res.Data = Helper.getStudentListExcel(fileRes.filePath);
                        res.Success = true;
                        res.StatusCode = HttpStatusCode.OK;
                    }
                    else
                    {
                        res.Message = Message.FileReadFail;
                        res.Data = null;
                        res.Success = true;
                        res.StatusCode = HttpStatusCode.OK;
                    }
                }
                else
                {
                    res.Message = Message.FileEmpty;
                    res.Data = null;
                    res.Success = false;
                    res.StatusCode = HttpStatusCode.BadRequest;
                }

            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e, Message.FileServerFail);
            }
            return res;
        }

        /// <summary>
        /// Xoá học sinh
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteStudent(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin"))
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 401;
                res.StatusCode = HttpStatusCode.Unauthorized;
                return res;
            }
            try
            {
                var studentObj = await _db.Student.FindAsync(id);
                if (studentObj == null)
                {
                    res.Message = Message.StudentNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;
                    return res;
                }

                _db.Student.Remove(studentObj);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = null;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Sửa học sinh
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditTeacher(Guid id, Student student)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 401;
                res.StatusCode = HttpStatusCode.Unauthorized;
                return res;
            }
            try
            {
                var studentResult = await _db.Student.FindAsync(id);
                if (studentResult == null)
                {
                    res.Message = Message.StudentNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }

                if (string.IsNullOrEmpty(student.StudentName))
                {
                    res.Message = Message.StudentNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                studentResult.StudentName = student.StudentName.Trim();
                studentResult.StudentImage = student.StudentImage?.Trim();
                studentResult.StudentPhone = student.StudentPhone?.Trim();
                studentResult.StudentDOB = student.StudentDOB?.Trim();
                studentResult.StudentGender = student.StudentGender?.Trim();
                studentResult.StudentDescription = student.StudentDescription?.Trim();
                studentResult.StudentAddress = student.StudentAddress?.Trim();
                studentResult.StudentImage = student.StudentImage?.Trim();
                studentResult.ShowStudentId = student.ShowStudentId?.Trim();
                studentResult.ModifyDate = DateTime.Now;

                var role = await _db.SystemRole.FindAsync(3);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("student", studentResult);
                result.Add("role", role);

                res.Success = true;
                res.Data = result;
                res.StatusCode = HttpStatusCode.OK;

                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }
    }
}
