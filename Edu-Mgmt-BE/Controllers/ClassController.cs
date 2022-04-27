using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Class;
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
    [Route("api/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;
        private const string TeacherClassQuery = "SELECT DISTINCT Class.*, (SELECT DISTINCT count(*) FROM HomeWorkClassDetail, HomeWork WHERE HomeWorkClassDetail.ClassId = Class.ClassId	AND HomeWork.HomeWorkId = HomeWorkClassDetail.HomeWorkId AND HomeWork.HomeWorkStatus = 1) as HomeWorkCount FROM Class WHERE Class.TeacherId = @teacherId";
        private const string TeacherClassQuerySearch = "SELECT DISTINCT Class.* FROM Class WHERE Class.TeacherId = @teacherId AND CHARINDEX(@txtSeach, ClassName) > 0";
        private const string StudentClassQuery = "SELECT DISTINCT Class.*, (SELECT DISTINCT COUNT(*) FROM HomeWorkClassDetail, HomeWork WHERE HomeWorkClassDetail.ClassId = Class.ClassId AND HomeWork.HomeWorkId = HomeWorkClassDetail.HomeWorkId AND HomeWork.HomeWorkStatus = 1) AS HomeWorkCount FROM Class, ClassDetail, Student WHERE ClassDetail.StudentId = Student.StudentId AND Class.ClassId = ClassDetail.ClassId AND Student.StudentName = @studentName AND Student.StudentDOB = @studentDob AND Student.StudentPhone = @studentPhone";
        private const string StudentInClassQuery = "SELECT Student.* FROM Class, ClassDetail, Student WHERE Class.ClassId = @classId AND ClassDetail.ClassId = Class.ClassId AND ClassDetail.StudentId = Student.StudentId";
        private const string FindStudentInClassQuery = "SELECT Student.* FROM Class, ClassDetail, Student WHERE Class.ClassId = @classId AND ClassDetail.ClassId = Class.ClassId AND ClassDetail.StudentId = Student.StudentId AND Student.StudentId = @studentId";

        public ClassController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách lớp có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/class?page=2&record=10&search=21
        [HttpGet]
        public async Task<ServiceResponse> GetClassByPagingAndSearch(
            [FromQuery] string search,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10,
            [FromQuery] int? classStatus = 1)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var role = Helper.getRole(HttpContext);
                List<ClassQuery> records = new List<ClassQuery>();

                if (role == RoleType.TEACHER)
                {
                    var teacherId = Helper.getTeacherId(HttpContext);
                    var paramId = new SqlParameter("@teacherId", teacherId);
                    if (search != null && search.Trim() != "")
                    {
                        var paramSearch = new SqlParameter("@txtSeach", search);
                        records = _db.ClassQuery
                            .FromSqlRaw(TeacherClassQuerySearch, paramId, paramSearch)
                            .OrderByDescending(x => x.ClassName)
                            .Where(x => x.ClassStatus == classStatus)
                            .ToList();
                    }
                    else
                    {
                        records = _db.ClassQuery
                            .FromSqlRaw(TeacherClassQuery, paramId)
                            .OrderByDescending(x => x.ClassName)
                            .Where(x => x.ClassStatus == classStatus)
                            .ToList();

                    }
                }
                else if (role == RoleType.STUDENT)
                {
                    var student = Helper.getStudentDetail(HttpContext);
                    var paramName = new SqlParameter("@studentName", student.StudentName);
                    var paramDob = new SqlParameter("@studentDob", student.StudentDob);
                    var paramPhone = new SqlParameter("@studentPhone", student.StudentPhone);

                    records = _db.ClassQuery
                            .FromSqlRaw(StudentClassQuery, paramName, paramDob, paramPhone)
                            .OrderByDescending(x => x.ClassName)
                            .Where(x => x.ClassStatus == classStatus)
                            .ToList();
                }

                PagingData pagingData = new PagingData()
                {
                    TotalRecord = records.Count(),
                    TotalPage = Convert.ToInt32(Math.Ceiling((decimal)records.Count() / (decimal)record.Value)),
                    Data = records?.Skip((page.Value - 1) * record.Value)?.Take(record.Value).ToList(),
                };
                res.Success = true;
                res.Data = pagingData;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Thêm lớp
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> AddClass(Class classObj)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var teacherId = Helper.getTeacherId(HttpContext);
                if (string.IsNullOrEmpty(teacherId?.ToString()))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherIdBadRequest);
                }
                var teacher_result = _db.Teacher.Find(teacherId);
                if (teacher_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.TeacherNotFound);
                }
                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassNameEmpty);
                }
                if (string.IsNullOrEmpty(classObj.ClassYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearDateEmpty);
                }
                int schoolYear;
                if (!int.TryParse(classObj.ClassYear, out schoolYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearBadRequest);
                }

                classObj.ClassId = Guid.NewGuid();
                classObj.ClassName = classObj.ClassName.Trim();
                classObj.ClassYear = schoolYear + "-" + (schoolYear + 1);
                classObj.TeacherId = teacherId.Value;
                classObj.ClassStatus = ClassStatus.Active;

                _db.Class.Add(classObj);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = classObj;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Chi tiết lớp
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetCLassDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var classResult = await _db.Class.FindAsync(id);
            if (classResult == null)
            {
                return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
            }
            Dictionary<string, object> result = new Dictionary<string, object>();

            var paramId = new SqlParameter("@classId", id);
            List<Student> studentList = _db.Student
                        .FromSqlRaw(StudentInClassQuery, paramId)
                        .OrderByDescending(x => x.StudentName)
                        .ToList();

            result.Add("class", classResult);
            result.Add("students", studentList);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Xoá lớp
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteClass(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var classResult = await _db.Class.FindAsync(id);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                _db.Class.Remove(classResult);
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
        /// Sửa lớp
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditClass(Guid id, Class classObj)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var classResult = await _db.Class.FindAsync(id);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassNameEmpty);
                }

                if (string.IsNullOrEmpty(classObj.ClassYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearDateEmpty);
                }
                int schoolYear;
                if (!int.TryParse(classObj.ClassYear, out schoolYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearBadRequest);
                }

                classResult.ClassName = classObj.ClassName.Trim();
                classResult.ClassYear = schoolYear + "-" + (schoolYear + 1);
                classResult.ClassStatus = classObj.ClassStatus;

                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = classResult;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Sửa trạng thái lớp
        /// </summary>
        /// <param name="classReq"></param>
        /// <returns></returns>
        [HttpPut("edit-status")]
        public async Task<ServiceResponse> EditClassStatus(ClassEditStatus classReq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var classResult = await _db.Class.FindAsync(classReq.ClassId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }

                classResult.ClassStatus = classReq.ClassStatus;
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = classResult;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
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
                if (string.IsNullOrEmpty(request.classId.ToString()))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
                }
                Dictionary<string, object> result = new Dictionary<string, object>();

                var class_result = _db.Class.Find(request.classId);
                if (class_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }
                result.Add("class", class_result);

                List<Student> studentDataList = new List<Student>();
                List<Student> newStudentDataList = new List<Student>();
                List<ClassDetail> classDetail = new List<ClassDetail>();

                foreach (var student in request.studentList)
                {
                    var student_result = _db.Student.Where(item =>
                    item.StudentName.ToLower().Equals(student.StudentName.Trim().ToLower()) &&
                    item.StudentDob.Equals(student.StudentDob.Trim()) &&
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
                        newStudentDataList.Add(student_result);
                    }
                    else
                    {
                        var student_check_exist = _db.ClassDetail
                       .Where(x => x.ClassId.Equals(request.classId))
                       .Where(x => x.StudentId.Equals(student_result.StudentId))
                       .FirstOrDefault();

                        if (student_check_exist != null)
                        {
                            return ErrorHandler.NotFoundResponse(Message.StudentExist + " Tên: " + student_result.StudentName);
                        }
                    }

                    studentDataList.Add(student_result);
                    classDetail.Add(new ClassDetail()
                    {
                        ClassDetailId = Guid.NewGuid(),
                        ClassId = request.classId,
                        StudentId = student_result.StudentId,
                    });
                }
                result.Add("studentDataList", studentDataList);
                //result.Add("classDetail", classDetail);

                _db.Student.AddRange(newStudentDataList);
                _db.ClassDetail.AddRange(classDetail);

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
        /// Edit học sinh trong lớp
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("edit-student/{classId}")]
        public async Task<ServiceResponse> EditStudentInClass(Guid classId, Student studentReq)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (classId == null || classId == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
                }
                Dictionary<string, object> result = new Dictionary<string, object>();

                var class_result = _db.Class.Find(classId);
                if (class_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }
                result.Add("class", class_result);

                var student_check = await _db.Student
                    .Where(x =>
                     x.StudentName.Equals(studentReq.StudentName.Trim())
                    && x.StudentDob.Equals(studentReq.StudentDob.Trim())
                    && x.StudentPhone.Equals(studentReq.StudentPhone.Trim())
                    ).FirstOrDefaultAsync();

                if (student_check.StudentId != studentReq.StudentId)
                {
                    student_check.StudentName = studentReq.StudentName.Trim();
                    student_check.StudentDob= studentReq.StudentDob.Trim();
                    student_check.StudentPhone = studentReq.StudentName.Trim();
                    student_check.StudentGender = studentReq.StudentGender.Trim();

                    result.Add("student_check", student_check);
                } else
                {
                    //var student_class = _db.ClassDetail.Where(x => x)
                }

                result.Add("student", studentReq);



                //await _db.SaveChangesAsync();

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
        /// Xóa học sinh trong lớp
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("remove-student")]
        public async Task<ServiceResponse> RemoveStudentFromClass(RemoveStudentClass request)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (request.classId == null || request.classId == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
                }
                Dictionary<string, object> result = new Dictionary<string, object>();

                var class_result = _db.Class.Find(request.classId);
                if (class_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }
                result.Add("class", class_result);

                List<ClassDetail> classDetail = new List<ClassDetail>();

                foreach (var studentId in request.studentList)
                {
                    var student_result = _db.ClassDetail
                         .Where(x => x.ClassId.Equals(request.classId))
                         .Where(x => x.StudentId.Equals(studentId))
                         .FirstOrDefault();

                    if (student_result == null)
                    {
                        return ErrorHandler.NotFoundResponse(Message.StudentNotFound);
                    }
                    classDetail.Add(student_result);
                }

                //result.Add("classDetail", classDetail);
                _db.ClassDetail.RemoveRange(classDetail);

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
        /// Check học sinh trong lớp theo id
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpPost("find-student-id")]
        public async Task<ServiceResponse> CheckClassStudent(CheckStudentById req)
        {
            ServiceResponse res = new ServiceResponse();

            if (string.IsNullOrEmpty(req.classId.ToString()))
            {
                return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
            }
            if (string.IsNullOrEmpty(req.studentId.ToString()))
            {
                return ErrorHandler.BadRequestResponse(Message.StudentEmpty);
            }

            var classResult = await _db.Class.FindAsync(req.classId);
            if (classResult == null)
            {
                return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();

            var paramClassId = new SqlParameter("@classId", req.classId);
            var paramStudentId = new SqlParameter("@studentId", req.studentId);

            Student student = await _db.Student
                        .FromSqlRaw(FindStudentInClassQuery, paramClassId, paramStudentId)
                        .OrderByDescending(x => x.StudentName)
                        .FirstOrDefaultAsync();

            result.Add("class", classResult);
            result.Add("student", student);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Check học sinh trong lớp
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpPost("find-student")]
        public async Task<ServiceResponse> CheckClassStudentByField(CheckStudentByField req)
        {
            ServiceResponse res = new ServiceResponse();

            if (string.IsNullOrEmpty(req.classId.ToString()))
            {
                return ErrorHandler.BadRequestResponse(Message.ClassEmpty);
            }

            var classResult = await _db.Class.FindAsync(req.classId);
            if (classResult == null)
            {
                return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();

            var paramId = new SqlParameter("@classId", req.classId);
            List<Student> studentList = _db.Student
                        .FromSqlRaw(StudentInClassQuery, paramId)
                        .OrderByDescending(x => x.StudentName)
                        .ToList();

            Student student = studentList.Where(item =>
            StringUtils.VietnameseNormalize(item.StudentName).Equals(StringUtils.VietnameseNormalize(req.studentName)) &&
            item.StudentPhone.Trim().Equals(req.studentPhone.Trim()) &&
            item.StudentDob.Trim().Equals(req.studentDob.Trim()))
                .FirstOrDefault();

            result.Add("class", classResult);
            result.Add("student", student);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

    }
}
