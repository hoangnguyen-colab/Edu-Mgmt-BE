using CsvHelper;
using CsvHelper.Configuration;
using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Class;
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
        private const string sql_get_student = "select * from Student where CHARINDEX(@txtSeach, StudentName) > 0 or CHARINDEX(@txtSeach, StudentDob) > 0 or CHARINDEX(@txtSeach, StudentPhone) > 0";

        private const string sql_get_class_by_teacher = "select * from Class c WHERE c.TeacherId = @teacherId AND c.status = 1";

        private const string sql_get_class_by_classIds = "select * from Class c WHERE c.ClassId IN @classIds AND c.status = 1";

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
        /// Lấy chi tiết thông tin học sinh theo sdt
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail-phone/{phone}")]
        public async Task<ServiceResponse> GetStudent(string phone)
        {
            ServiceResponse res = new ServiceResponse();
            var student = await _db.Student
                .Where(x => x.StudentPhone.Equals(phone.Trim()))
                .FirstOrDefaultAsync();

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
                    var fileRes = FileUtils.saveFile(HttpContext, file);
                    if (fileRes.success)
                    {
                        res.Data = FileUtils.getStudentListExcel(fileRes.filePath);
                        res.Success = true;
                        res.StatusCode = HttpStatusCode.OK;

                        FileUtils.removeFile(fileRes.filePath);
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
        /// Thêm học sinh vào lớp
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPost("add-student")]
        public async Task<ServiceResponse> AddStudentToClass(AddStudentToClassRequest request)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                List<Student> studentDataList = new List<Student>();

                foreach (var student in request.studentList)
                {
                    var student_result = _db.Student.Where(item =>
                    item.StudentPhone.Equals(student.StudentPhone.Trim())).FirstOrDefault();

                    if (student_result == null)
                    {
                        student_result = new Student()
                        {
                            StudentId = Guid.NewGuid(),
                            StudentName = student.StudentName.Trim(),
                            StudentDob = student.StudentDob.Trim(),
                            StudentGender = student.StudentGender.Trim(),
                            StudentPhone = student.StudentPhone.Trim(),
                        };

                        studentDataList.Add(student_result);
                    }
                    else
                    {
                        return ErrorHandler.NotFoundResponse(Message.StudentExist + " Sđt: " + student_result.StudentPhone);
                    }

                }
                result.Add("studentDataList", studentDataList);
                _db.Student.AddRange(studentDataList);

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
        public async Task<ServiceResponse> EditStudent(Guid id, Student student)
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
                studentResult.StudentPhone = student.StudentPhone?.Trim();
                studentResult.StudentDob = student.StudentDob?.Trim();
                studentResult.StudentGender = student.StudentGender?.Trim();

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

        /// <summary>
        /// Submit lớp học
        /// </summary>
        /// <param name="SubmitClassRequest"></param>
        /// <returns></returns>
        [HttpPost("submit/{id}")]
        public async Task<ServiceResponse> SubmitClass(SubmitClassRequest req)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "student"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                List<Class> classList = new List<Class>();
                classList = _db.Class.FromSqlRaw(sql_get_class_by_classIds, req.classIds).ToList();
                var studentResult = await _db.Student.FindAsync(req.studentId);
                if (studentResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.StudentNotFound);
                }
                if (classList == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassSubmit);
                }
                if (classList != null)
                {
                    foreach (var item in classList)
                    {
                        ClassRequest cr = new ClassRequest();
                        cr.ClassRequestId = Guid.NewGuid();
                        cr.ClassId = item.ClassId;
                        cr.StudentId = req.studentId;
                        cr.RequestDate = new DateTime();
                        cr.RequestStatus = ClassRequestStatus.Pending;
                        cr.RequestContent = req.content.Trim();
                        _db.ClassRequest.Add(cr);
                    }
                }

                await _db.SaveChangesAsync();

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("student", studentResult);
                res.Message = "Đăng ký vào lớp học thành công!";
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
    }
}
