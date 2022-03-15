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
                    string sql_get_account = "select * from Teacher where " +
                        "CHARINDEX(@txtSeach, TeacherName) > 0 or " +
                        "CHARINDEX(@txtSeach, TeacherEmail) or" +
                        "CHARINDEX(@txtSeach, TeacherPhone) or" +
                        "CHARINDEX(@txtSeach, ShowTeacherId) or" +
                        "CHARINDEX(@txtSeach, TeacherAddress)";

                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.Teacher
                        .FromSqlRaw(sql_get_account, param)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();
                }
                else
                {
                    records = await _db.Teacher
                        .OrderByDescending(x => x.CreatedDate)
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
                result.Add("teacher-account", teacherAccount);
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
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (string.IsNullOrEmpty(teacher.TeacherName))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherNameEmpty);
                }

                teacher.TeacherId = Guid.NewGuid();
                teacher.TeacherName = teacher.TeacherName.Trim();
                teacher.ShowTeacherId = Helper.GenerateTeacherID(_db.Teacher.Count() + 1);
                _db.Teacher.Add(teacher);

                SystemUser sysUser = new SystemUser
                {
                    SystemUserId = Guid.NewGuid(),
                    Username = teacher.TeacherName,
                    UserUsername = teacher.ShowTeacherId.ToLower(),
                    UserPassword = Helper.EncodeMD5(teacher.ShowTeacherId.ToLower()),
                };
                _db.SystemUser.Add(sysUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = teacher.TeacherId,
                    SystemRoleId = 2,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(2);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("teacher", teacher);
                result.Add("teacherAccount", sysUser);
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
                teacherResult.TeacherImage = teacher.TeacherImage?.Trim();
                teacherResult.TeacherEmail = teacher.TeacherEmail?.Trim();
                teacherResult.TeacherPhone = teacher.TeacherPhone?.Trim();
                teacherResult.TeacherAddress = teacher.TeacherAddress?.Trim();
                teacherResult.ModifyDate = DateTime.Now;

                var role = await _db.SystemRole.FindAsync(2);

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
