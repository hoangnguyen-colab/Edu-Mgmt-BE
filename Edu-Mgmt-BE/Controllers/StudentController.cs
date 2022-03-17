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
        private const string sql_get_student = "select * from Student where " +
              "CHARINDEX(@txtSeach, StudentName) > 0 or " +
              "CHARINDEX(@txtSeach, TeacherEmail) or" +
              "CHARINDEX(@txtSeach, StudentGender) or" +
              "CHARINDEX(@txtSeach, StudentDOB)";

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
                    return ErrorHandler.UnauthorizeCatchResponse();
                }
                var pagingData = new PagingData();
                List<Student> records = new List<Student>();

                if (search != null && search.Trim() != "")
                {
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.Student
                        .FromSqlRaw(sql_get_student, param)
                        .OrderByDescending(x => x.StudentName)
                        .ToList();
                }
                else
                {
                    records = await _db.Student
                        .OrderByDescending(x => x.StudentName)
                        .ToListAsync();
                }

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
                return ErrorHandler.NotFoundResponse(Message.StudentNotFound);
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
            return await UserCreator.StudentCreate(studentReq, "");
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
                return ErrorHandler.UnauthorizeCatchResponse();
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
                    res = ErrorHandler.NotFoundResponse(Message.FileEmpty);
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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var studentObj = await _db.Student.FindAsync(id);
                if (studentObj == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.StudentNotFound);
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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var studentResult = await _db.Student.FindAsync(id);
                if (studentResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.StudentNotFound);
                }

                if (string.IsNullOrEmpty(student.StudentName))
                {
                    return ErrorHandler.BadRequestResponse(Message.StudentNameEmpty);
                }

                studentResult.StudentName = student.StudentName.Trim();
                studentResult.StudentImage = student.StudentImage?.Trim();
                studentResult.StudentPhone = student.StudentPhone?.Trim();
                studentResult.StudentDob = student.StudentDob?.Trim();
                studentResult.StudentGender = student.StudentGender?.Trim();
                studentResult.StudentImage = student.StudentImage?.Trim();

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
