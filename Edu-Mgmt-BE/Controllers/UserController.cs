using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.User;
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
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        public UserController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách tài khoản có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/accounts?page=2&record=10&search=admin
        [HttpGet]
        public async Task<ServiceResponse> GetAccountsByPagingAndSearch([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            if (Helper.CheckPermission(HttpContext, "admin"))
            {
                var pagingData = new PagingData();
                List<SystemUser> records = new List<SystemUser>();
                //Tổng số bản ghi
                if (search != null && search.Trim() != "")
                {
                    //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                    string sql_get_account = "select * from SystemUser where CHARINDEX(@txtSeach, Username) > 0 or CHARINDEX(@txtSeach, UserUsername) > 0";
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.SystemUser.FromSqlRaw(sql_get_account, param).OrderByDescending(x => x.CreatedDate).ToList();
                }
                else
                {
                    records = await _db.SystemUser.OrderByDescending(x => x.CreatedDate).ToListAsync();
                }
                pagingData.TotalRecord = records.Count(); //Tổng số bản ghi
                pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value)); //Tổng số trang
                pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(); //Dữ liệu của từng trang
                res.Success = true;
                res.Data = pagingData;
                res.StatusCode = HttpStatusCode.OK;
                return res;
            }
            res.Success = false;
            res.Message = Message.NotAuthorize;
            res.ErrorCode = 401;
            res.StatusCode = HttpStatusCode.Unauthorized;
            return res;
        }
        
        /// <summary>
        /// Lấy chi tiết thông tin tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var account = await _db.SystemUser.FindAsync(id);
            if (account == null)
            {
                res.Data = null;
                res.Message = Message.AccountNotFound;
                res.ErrorCode = 404;
                res.StatusCode = HttpStatusCode.NotFound;
                return res;
            }
            account.UserPassword = null;
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("account", account);

            string sql_get_role = $"select * from SystemRole where RoleId in (select distinct SystemRoleId from UserDetail where SystemUserId = @SystemUserId)";
            var roles = await _db.SystemRole
                .FromSqlRaw(sql_get_role, new SqlParameter("@SystemUserId", account.SystemUserId))
                .ToListAsync();
            result.Add("roles", roles);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param username="" password=""></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ServiceResponse> UserLogin(UserLogin userLogin)
        {
            return await _jwtAuthenticationManager.LoginAuthenticate(_db, userLogin.username, userLogin.password);
        }

        /// <summary>
        /// Tạo mới admin
        /// </summary>
        /// <param SystemUser="SystemUser"></param>
        /// <returns></returns>
        [HttpPost("create-user")]
        public async Task<ServiceResponse> CreateAdmin(SystemUser systemUser)
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
                if (string.IsNullOrEmpty(systemUser.Username))
                {
                    res.Message = Message.UserNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                if (string.IsNullOrEmpty(systemUser.UserUsername))
                {
                    res.Message = Message.UserLoginNameEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                if (string.IsNullOrEmpty(systemUser.UserPassword))
                {
                    res.Message = Message.UserPasswordEmpty;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }
                var find_user = await _db.SystemUser
                  .Where(item => item.UserUsername.Equals(systemUser.UserUsername))
                  .FirstOrDefaultAsync();
                if (find_user != null)
                {
                    res.Message = Message.UserLoginNameExist;
                    res.Success = false;
                    res.ErrorCode = 400;
                    res.StatusCode = HttpStatusCode.BadRequest;

                    return res;
                }

                systemUser.SystemUserId = Guid.NewGuid();
                systemUser.Username = systemUser.Username.Trim();
                systemUser.UserUsername = systemUser.UserUsername.Trim();
                systemUser.UserPassword = Helper.EncodeMD5(systemUser.UserPassword.Trim());
                _db.SystemUser.Add(systemUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = null,
                    SystemRoleId = 1,
                    SystemUserId = systemUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(1);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("systemUser", systemUser);
                result.Add("userDetail", sysUserDetail);
                result.Add("role", role);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = result;
                res.StatusCode = HttpStatusCode.OK;

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
    }
}
