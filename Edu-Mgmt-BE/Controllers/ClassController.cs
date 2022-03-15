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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassNameEmpty);
                }

                //if (string.IsNullOrEmpty(classObj.ShowClassId))
                //{
                //    return ErrorHandler.BadRequestResponse(Message.ClassIdEmpty);
                //}

                classObj.ClassId = Guid.NewGuid();
                classObj.ClassName = classObj.ClassName.Trim();
                classObj.ShowClassId = classObj.ClassName;

                var find_class = await _db.Class
                    .Where(item => item.ClassName.Equals(classObj.ClassName))
                    .FirstOrDefaultAsync();
                if (find_class != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassExist);
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
                return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
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

                if (string.IsNullOrEmpty(classObj.ClassName))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassNameEmpty);
                }

                var find_class = await _db.Class
                    .Where(item => item.ClassName.Equals(classObj.ClassName))
                    .FirstOrDefaultAsync();
                if (find_class != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassExist);
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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if(string.IsNullOrEmpty(classDetail.ClassId.ToString()))
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassIdEmpty);
                }
                if (string.IsNullOrEmpty(classDetail.TeacherId.ToString()))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherIdEmpty);
                }
                
                var classResult = await _db.Class.FindAsync(classDetail.ClassId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                }
                var yearResult = await _db.SchoolYear.FindAsync(classDetail.SchoolYearId);
                if (classResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.SchoolYearNotFound);
                }

                var teacherResult = await _db.Teacher.FindAsync(classDetail.TeacherId);
                if (teacherResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.TeacherNotFound);
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
