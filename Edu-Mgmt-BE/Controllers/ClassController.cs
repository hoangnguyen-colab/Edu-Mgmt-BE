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
    [Route("api/class")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;
        private const string TeacherClassQuery = "SELECT DISTINCT Class.* FROM Class JOIN ClassDetail  ON Class.ClassId = ClassDetail.ClassId JOIN Teacher ON ClassDetail.TeacherId = @teacherId";
        private const string TeacherClassQuerySearch = "SELECT * FROM  (SELECT DISTINCT Class.* FROM Class JOIN ClassDetail  ON Class.ClassId = ClassDetail.ClassId JOIN Teacher ON ClassDetail.TeacherId = @teacherId) class where CHARINDEX(@txtSeach, ClassName) > 0 OR CHARINDEX(@txtSeach, ShowClassId) > 0";
        private const string SearchClassQuery = "SELECT * FROM Class WHERE CHARINDEX(@txtSeach, ClassName) > 0 OR CHARINDEX(@txtSeach, ShowClassId) > 0";

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
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var pagingData = new PagingData();
                List<Class> records = new List<Class>();

                string role = Helper.getRole(HttpContext);
                if (role.Equals("teacher"))
                {
                    var teacherId = Helper.getTeacherId(HttpContext);
                    if (search != null && search.Trim() != "")
                    {
                        var paramId = new SqlParameter("@teacherId", teacherId);
                        var paramSearch = new SqlParameter("@txtSeach", search);
                        records = _db.Class.FromSqlRaw(TeacherClassQuerySearch, paramId, paramSearch).OrderByDescending(x => x.CreatedDate).ToList();
                    }
                    else
                    {
                        var paramId = new SqlParameter("@teacherId", teacherId);
                        records = _db.Class.FromSqlRaw(TeacherClassQuery, paramId).OrderByDescending(x => x.CreatedDate).ToList();
                    }
                }
                else
                {
                    if (search != null && search.Trim() != "")
                    {
                        var param = new SqlParameter("@txtSeach", search);
                        records = _db.Class.FromSqlRaw(SearchClassQuery, param).OrderByDescending(x => x.CreatedDate).ToList();
                    }
                    else
                    {
                        records = await _db.Class.OrderByDescending(x => x.CreatedDate).ToListAsync();
                    }
                }

                pagingData.TotalRecord = records.Count(); //Tổng số bản ghi
                pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value)); //Tổng số trang
                pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(); //Dữ liệu của từng trang

                res.Success = true;
                res.StatusCode = HttpStatusCode.OK;
                res.Data = pagingData;
            }
            catch (Exception e)
            {
                res.Message = Message.ErrorMsg;
                res.Success = false;
                res.ErrorCode = 500;
                res.StatusCode = HttpStatusCode.InternalServerError;
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
                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    res.Message = Message.ClassNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                //if (string.IsNullOrEmpty(classObj.ShowClassId))
                //{
                //    res.Message = Message.ClassIdEmpty;
                //    res.Success = false;
                //    res.ErrorCode = 400;
                //    res.StatusCode = HttpStatusCode.BadRequest;

                //    return res;
                //}

                classObj.ClassId = Guid.NewGuid();
                classObj.ClassName = classObj.ClassName.Trim();
                classObj.ShowClassId = classObj.ClassName;

                var find_class = await _db.Class
                    .Where(item => item.ClassName.Equals(classObj.ClassName))
                    .FirstOrDefaultAsync();
                if (find_class != null)
                {
                    res.Message = Message.ClassExist;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

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
                res.Message = Message.ClassNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
                res.StatusCode = HttpStatusCode.NotFound;

                return res;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("class", classResult);

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
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 401;
                res.StatusCode = HttpStatusCode.Unauthorized;
                return res;
            }
            try
            {
                var classResult = await _db.Class.FindAsync(id);
                if (classResult == null)
                {
                    res.Message = Message.ClassNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;
                    return res;
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
                var classResult = await _db.Class.FindAsync(id);
                if (classResult == null)
                {
                    res.Message = Message.ClassNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }

                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    res.Message = Message.ClassNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                var find_year = await _db.Class
                    .Where(item => item.ClassName.Equals(classObj.ClassName))
                    .FirstOrDefaultAsync();
                if (find_year != null)
                {
                    res.Message = Message.ClassExist;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                classResult.ClassName = classObj.ClassName.Trim();
                classResult.ShowClassId = classObj.ClassName.Trim();
                classResult.ModifyDate = DateTime.Now;

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
        /// Phân lớp
        /// </summary>
        /// <param name="ClassDetail"></param>
        /// <returns></returns>
        [HttpPost("assign-class")]
        public async Task<ServiceResponse> AssignClass(ClassDetail classDetail)
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
                if(string.IsNullOrEmpty(classDetail.ClassId.ToString()))
                {
                    res.Message = Message.ClassIdEmpty;
                    res.ErrorCode = 400;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                if (string.IsNullOrEmpty(classDetail.TeacherId.ToString()))
                {
                    res.Message = Message.TeacherIdEmpty;
                    res.ErrorCode = 400;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                
                var classResult = await _db.Class.FindAsync(classDetail.ClassId);
                if (classResult == null)
                {
                    res.Message = Message.ClassNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }
                var yearResult = await _db.SchoolYear.FindAsync(classDetail.SchoolYearId);
                if (classResult == null)
                {
                    res.Message = Message.SchoolYearNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }

                var teacherResult = await _db.Teacher.FindAsync(classDetail.TeacherId);
                if (teacherResult == null)
                {
                    res.Message = Message.TeacherNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }

                classDetail.StudentId = null;
                classDetail.ClassDetailId = Guid.NewGuid();

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("classResult", classResult);
                result.Add("teacherResult", teacherResult);
                result.Add("classDetail", classDetail);

                _db.ClassDetail.Add(classDetail);
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
