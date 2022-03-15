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
    [Route("api/school-year")]
    [ApiController]
    public class SchoolYearController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        public SchoolYearController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách category có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/school-year?page=2&record=10&search=21
        [HttpGet]
        public async Task<ServiceResponse> GetCategoriesByPagingAndSearch(
            [FromQuery] string search,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var pagingData = new PagingData();
                List<SchoolYear> records = new List<SchoolYear>();
                //Tổng số bản ghi
                if (search != null && search.Trim() != "")
                {
                    //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                    string sql_get_year = "SELECT * FROM SchoolYear WHERE CHARINDEX(@txtSeach, SchoolYearName) > 0 OR CHARINDEX(@txtSeach, SchoolYearDate) > 0";
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.SchoolYear.FromSqlRaw(sql_get_year, param).OrderByDescending(x => x.SchoolYearName).ToList();
                }
                else
                {
                    records = await _db.SchoolYear.OrderByDescending(x => x.SchoolYearName).ToListAsync();
                }
                pagingData.TotalRecord = records.Count(); //Tổng số bản ghi
                pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value)); //Tổng số trang
                pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(); //Dữ liệu của từng trang

                res.Success = true;
                res.StatusCode = HttpStatusCode.OK;
                res.Data = pagingData;
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
        /// Thêm school year
        /// </summary>
        /// <param name="schoolYear"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> AddSchoolYear(SchoolYear schoolYear)
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
                if (string.IsNullOrEmpty(schoolYear.ActiveYear))
                {
                    res.Message = Message.SchoolYearDateEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                var find_year = await _db.SchoolYear.Where(item => item.ActiveYear.Equals(schoolYear.ActiveYear)).FirstOrDefaultAsync();
                if (find_year != null)
                {
                    res.Message = Message.SchoolYearExist;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                schoolYear.SchoolYearId = Guid.NewGuid();
                schoolYear.ActiveYear = schoolYear.ActiveYear.Trim();
                schoolYear.SchoolYearName = "Khóa " + schoolYear.ActiveYear;
                schoolYear.SchoolYearDate = schoolYear.ActiveYear + "-" + (Int32.Parse(schoolYear.ActiveYear) + 1);

                _db.SchoolYear.Add(schoolYear);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = schoolYear;
                res.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                res = ErrorHandler.ErrorCatchResponse(e);
            }
            return res;
        }

        /// <summary>
        /// Detail school year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetSchoolYearDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var year = await _db.SchoolYear.FindAsync(id);
            if (year == null)
            {
                res.Message = Message.SchoolYearNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
                res.StatusCode = HttpStatusCode.NotFound;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("year", year);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Xoá year
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteSchoolYear(Guid? id)
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
                var year = await _db.SchoolYear.FindAsync(id);
                if (year == null)
                {
                    res.Message = Message.SchoolYearNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;
                    return res;
                }
                _db.SchoolYear.Remove(year);
                res.Success = true;
                res.Data = null;
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
        /// Sửa năm học
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditSchoolYear(Guid id, SchoolYear schoolYear)
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
                var yearResult = await _db.SchoolYear.FindAsync(id);
                if (yearResult == null)
                {
                    res.Message = Message.SchoolYearNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    res.StatusCode = HttpStatusCode.NotFound;

                    return res;
                }

                if (string.IsNullOrEmpty(schoolYear.ActiveYear))
                {
                    res.Message = Message.SchoolYearDateEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                var find_year = await _db.SchoolYear
                    .Where(item => item.ActiveYear.Equals(schoolYear.ActiveYear) && !item.ActiveYear.Equals(yearResult.ActiveYear))
                    .FirstOrDefaultAsync();
                if (find_year != null)
                {
                    res.Message = Message.SchoolYearExist;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                yearResult.ActiveYear = schoolYear.ActiveYear.Trim();
                yearResult.SchoolYearName = schoolYear.SchoolYearName.Trim();
                yearResult.SchoolYearDate = schoolYear.ActiveYear + "-" + (Int32.Parse(schoolYear.ActiveYear) + 1);
                yearResult.ModifyDate = DateTime.Now;

                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = yearResult;
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
