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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                if (string.IsNullOrEmpty(schoolYear.ActiveYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearDateEmpty);
                }

                var find_year = await _db.SchoolYear.Where(item => item.ActiveYear.Equals(schoolYear.ActiveYear)).FirstOrDefaultAsync();
                if (find_year != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearExist);
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
                return ErrorHandler.NotFoundResponse(Message.SchoolYearNotFound);
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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var year = await _db.SchoolYear.FindAsync(id);
                if (year == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.SchoolYearNotFound);
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
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                var yearResult = await _db.SchoolYear.FindAsync(id);
                if (yearResult == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.SchoolYearNotFound);
                }

                if (string.IsNullOrEmpty(schoolYear.ActiveYear))
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearDateEmpty);
                }

                var find_year = await _db.SchoolYear
                    .Where(item => item.ActiveYear.Equals(schoolYear.ActiveYear) && !item.ActiveYear.Equals(yearResult.ActiveYear))
                    .FirstOrDefaultAsync();
                if (find_year != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.SchoolYearExist);
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
