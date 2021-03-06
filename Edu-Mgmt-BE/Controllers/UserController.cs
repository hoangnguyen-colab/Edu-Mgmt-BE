using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Student;
using Edu_Mgmt_BE.Models.CustomModel.User;
using Edu_Mgmt_BE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            try
            {
                if (!Helper.CheckPermission(HttpContext, "admin"))
                {
                    return ErrorHandler.UnauthorizeCatchResponse();
                }
                List<SystemUser> records = new List<SystemUser>();

                if (search != null && search.Trim() != "")
                {
                    //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                    string sql_get_account = "select * from SystemUser where CHARINDEX(@txtSeach, Username) > 0 or CHARINDEX(@txtSeach, UserUsername) > 0";
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.SystemUser.FromSqlRaw(sql_get_account, param).OrderByDescending(x => x.Fullname).ToList();
                }
                else
                {
                    records = await _db.SystemUser.OrderByDescending(x => x.Fullname).ToListAsync();
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
        /// Lấy chi tiết thông tin tài khoản đang dùng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        public async Task<ServiceResponse> GetAccount()
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var userId = Helper.getUserId(HttpContext);
                var account = await _db.SystemUser.FindAsync(userId);
                var account_detail = await _db.UserDetail
                    .Where(x => x.SystemUserId.Equals(account.SystemUserId))
                    .FirstOrDefaultAsync();
                if (account == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.AccountNotFound);
                }
                account.UserPassword = null;
                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("account", account);

                string sql_get_role = $"select * from SystemRole where RoleId in (select distinct SystemRoleId from UserDetail where SystemUserId = @SystemUserId)";
                var roles = await _db.SystemRole
                    .FromSqlRaw(sql_get_role, new SqlParameter("@SystemUserId", account.SystemUserId))
                    .ToListAsync();

                if (roles[0]?.RoleId == RoleType.TEACHER)
                {
                    var teacher = await _db.Teacher.FindAsync(account_detail.UserId);
                    result.Add("teacher", teacher);
                }
                //else if (roles[0]?.RoleId == RoleType.STUDENT)
                //{
                //    var student = await _db.Student.FindAsync(account_detail.UserId);
                //    result.Add("student", student);
                //}
                result.Add("roles", roles);

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
        [AllowAnonymous]
        [HttpPost("sign-up")]
        public async Task<ServiceResponse> CreateUser(UserSignUp userSignUp)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                DateTime date;
                if (!DateTime.TryParseExact(userSignUp.UserDOB?.Trim(), "dd'/'MM'/'yyyy",
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out date))
                {
                    return ErrorHandler.BadRequestResponse(Message.InvalidDOB);
                }

                string phoneNumber = userSignUp.UserPhone?.Trim();
                if (!StringUtils.IsPhoneNumber(phoneNumber))
                {
                    return ErrorHandler.BadRequestResponse(Message.InvalidPhone);
                }
                if (phoneNumber.Substring(0, 3) == "+84")
                {
                    userSignUp.UserPhone = userSignUp.UserPhone.Trim().Replace("+84", "0");
                }

                if (userSignUp.signUpUserType == RoleType.TEACHER) //teacher
                {
                    Teacher teacher = new Teacher()
                    {
                        TeacherName = userSignUp.UserName?.Trim(),
                        TeacherPhone = userSignUp.UserPhone?.Trim(),
                        TeacherEmail = userSignUp.UserEmail?.Trim(),
                        TeacherGender = userSignUp.UserGender?.Trim(),
                        TeacherDob = userSignUp.UserDOB?.Trim(),
                    };
                    return await UserCreator.TeacherCreate(teacher, userSignUp.UserPassword);
                }
                if (userSignUp.signUpUserType == RoleType.STUDENT) //student
                {
                    AddStudentRequest student = new AddStudentRequest()
                    {
                        StudentName = userSignUp.UserName?.Trim(),
                        StudentPhone = userSignUp.UserPhone?.Trim(),
                        StudentDob = userSignUp.UserDOB,
                        StudentGender = userSignUp.UserGender?.Trim(),
                    };
                    return await UserCreator.StudentCreate(student, userSignUp.UserPassword);
                }

                res.Data = null;
                res.Success = false;
                return res;
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param SystemUser="SystemUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("change-password")]
        public async Task<ServiceResponse> ChangePassword(string oldPassword, string newPassword)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var userId = Helper.getUserId(HttpContext);
                string newPasswordMD5 = Helper.EncodeMD5(newPassword);
                var accountResult = await _db.SystemUser
                    .Where(_ => _.SystemUserId.Equals(userId) && _.UserPassword.Equals(oldPassword))
                    .FirstOrDefaultAsync();
                if (accountResult == null)
                {
                    return ErrorHandler.BadRequestResponse(Constants.Message.LoginIncorrect);
                }
                accountResult.UserPassword = newPasswordMD5;
                _db.SystemUser.Add(accountResult);
                await _db.SaveChangesAsync();

                res.Data = null;
                res.Success = false;
                return res;
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }

        /// <summary>
        /// Edit profile
        /// </summary>
        /// <param SystemUser="SystemUser"></param>
        /// <returns></returns>
        [HttpPost("edit-profile")]
        public async Task<ServiceResponse> EditProfile(UserSignUp userSignUp)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                var userId = Helper.getUserId(HttpContext);
                var role = Helper.getRole(HttpContext);
                var account = await _db.SystemUser.FindAsync(userId);
                var account_detail = await _db.UserDetail
                    .Where(x => x.SystemUserId.Equals(account.SystemUserId))
                    .FirstOrDefaultAsync();
                if (account == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.AccountNotFound);
                }
                DateTime date;
                if (!DateTime.TryParseExact(userSignUp.UserDOB?.Trim(), "dd'/'MM'/'yyyy",
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out date))
                {
                    return ErrorHandler.BadRequestResponse(Message.InvalidDOB);
                }

                string phoneNumber = userSignUp.UserPhone?.Trim();
                if (!StringUtils.IsPhoneNumber(phoneNumber))
                {
                    return ErrorHandler.BadRequestResponse(Message.InvalidPhone);
                }
                if (phoneNumber.Substring(0, 3) == "+84")
                {
                    userSignUp.UserPhone = userSignUp.UserPhone.Trim().Replace("+84", "0");
                }

                account.Fullname = userSignUp.UserName?.Trim();
                account.UserDob = userSignUp.UserDOB?.Trim();
                account.UserGender = userSignUp.UserGender?.Trim();
                account.UserPhone = userSignUp.UserPhone?.Trim();
                account.UserUsername = userSignUp.UserPhone?.Trim();

                result.Add("account", account);
                if (role == RoleType.TEACHER)
                {
                    var teacher = await _db.Teacher.FindAsync(account_detail.UserId);

                    if  (teacher != null)
                    {
                        teacher.TeacherName = userSignUp.UserName?.Trim();
                        teacher.TeacherPhone = userSignUp.UserPhone?.Trim();
                        teacher.TeacherEmail = userSignUp.UserEmail?.Trim();
                        teacher.TeacherGender = userSignUp.UserGender?.Trim();
                        teacher.TeacherDob = userSignUp.UserDOB?.Trim();
                    }

                    result.Add("teacher", teacher);
                }

                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = result;
                res.StatusCode = HttpStatusCode.OK;
                return res;
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }
    }
}
