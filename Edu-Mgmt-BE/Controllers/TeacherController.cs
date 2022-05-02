using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
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
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;
        private const string sql_get_teacher = "select * from Teacher where " +
            "CHARINDEX(@txtSeach, TeacherName) > 0 or " +
            "CHARINDEX(@txtSeach, TeacherEmail) or" +
            "CHARINDEX(@txtSeach, TeacherPhone) or" +
            "CHARINDEX(@txtSeach, TeacherGender) or" +
            "CHARINDEX(@txtSeach, TeacherDOB)";
        public TeacherController(EduManagementContext context,
            IJwtAuthenticationManager jwtAuthenticationManager
            )
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách giáo viên có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/accounts?page=2&record=10&search=admin
        [HttpGet]
        public async Task<ServiceResponse> GetTeacherByPagingAndSearch(
            [FromQuery] string search,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (!Helper.CheckPermission(HttpContext, "admin"))
                {
                    return ErrorHandler.UnauthorizeCatchResponse();
                }
                var pagingData = new PagingData();
                List<Teacher> records = new List<Teacher>();

                if (search != null && search.Trim() != "")
                {

                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.Teacher
                        .FromSqlRaw(sql_get_teacher, param)
                        .OrderByDescending(x => x.TeacherName)
                        .ToList();
                }
                else
                {
                    records = await _db.Teacher
                        .OrderByDescending(x => x.TeacherName)
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
        /// Lấy chi tiết thông tin giáo viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetTeacher(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
                {
                    return ErrorHandler.UnauthorizeCatchResponse();
                }
                var teacher = await _db.Teacher.FindAsync(id);
                if (teacher == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.TeacherNotFound);
                }
                var userDetail = await _db.UserDetail
                    .Where(item => item.UserId.Equals(teacher.TeacherId))
                    .FirstOrDefaultAsync();

                var teacherAccount = await _db.SystemUser
                    .Where(item => item.SystemUserId.Equals(userDetail.SystemUserId))
                    .FirstOrDefaultAsync();
                teacherAccount.UserPassword = "";

                Dictionary<string, object> result = new Dictionary<string, object>();
                var role = await _db.SystemRole.FindAsync(2);

                result.Add("teacher", teacher);
                result.Add("teacherAccount", teacherAccount);
                result.Add("role", role);

                res.Data = result;
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
        /// Thêm giáo viên
        /// </summary>
        /// <param name="teacher"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> AddTeacher(Teacher teacher)
        {
            return await UserCreator.TeacherCreate(teacher, "");
        }

        /// <summary>
        /// Xoá giáo viên
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteTeacher(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var teacherObj = await _db.Teacher.FindAsync(id);
                if (teacherObj == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.TeacherNotFound);
                }

                _db.Teacher.Remove(teacherObj);
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
        /// Sửa giáo viên
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditTeacher(Guid id, Teacher teacher)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var teacherResult = await _db.Teacher.FindAsync(id);
                if (teacherResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.TeacherNotFound);
                }

                if (string.IsNullOrEmpty(teacher.TeacherName))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherNameEmpty);
                }

                teacherResult.TeacherName = teacher.TeacherName.Trim();
                teacherResult.TeacherEmail = teacher.TeacherEmail?.Trim();

                var role = await _db.SystemRole.FindAsync(RoleType.TEACHER);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("teacher", teacherResult);
                result.Add("role", role);

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
    }
}
