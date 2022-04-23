using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Student;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Utils
{
    public class UserCreator
    {
        private static readonly EduManagementContext _db = new EduManagementContext();
        private const string StudentCheckAccount = @"SELECT UserDetail.* FROM Student, UserDetail
        WHERE Student.StudentId = UserDetail.UserId
        AND Student.StudentPhone = @studentPhone";

        public static async Task<ServiceResponse> AdminCreate(SystemUser systemUser)
        {
            try
            {
                if (string.IsNullOrEmpty(systemUser.Username))
                {
                    return ErrorHandler.BadRequestResponse(Message.UserNameEmpty);
                }
                if (string.IsNullOrEmpty(systemUser.UserUsername))
                {
                    return ErrorHandler.BadRequestResponse(Message.UserLoginNameEmpty);
                }
                if (string.IsNullOrEmpty(systemUser.UserPassword))
                {
                    return ErrorHandler.BadRequestResponse(Message.UserPasswordEmpty);
                }
                var find_user = await _db.SystemUser
                  .Where(item => item.UserUsername.Equals(systemUser.UserUsername))
                  .FirstOrDefaultAsync();
                if (find_user != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.UserLoginNameExist);
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

                return new ServiceResponse()
                {
                    Success = true,
                    Data = result,
                    StatusCode = HttpStatusCode.OK,
                }; ;
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }

        public static async Task<ServiceResponse> TeacherCreate(Teacher teacher, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(teacher.TeacherName))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherNameEmpty);
                }
                if (string.IsNullOrEmpty(teacher.TeacherPhone))
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherPhoneEmpty);
                }
                var teacher_result = _db.Teacher
                    .Where(item => item.TeacherPhone.Equals(teacher.TeacherPhone.Trim()))
                    .FirstOrDefault();
                if (teacher_result != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherPhoneExist);
                }

                teacher.TeacherId = Guid.NewGuid();
                teacher.TeacherName = teacher.TeacherName.Trim();
                teacher.TeacherPhone = teacher.TeacherPhone.Trim();
                _db.Teacher.Add(teacher);

                SystemUser sysUser = new SystemUser
                {
                    SystemUserId = Guid.NewGuid(),
                    Username = teacher.TeacherName,
                    UserUsername = teacher.TeacherPhone,
                    UserPassword = (string.IsNullOrEmpty(password)) ? Helper.EncodeMD5(teacher.TeacherPhone) : Helper.EncodeMD5(password),
                };
                _db.SystemUser.Add(sysUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = teacher.TeacherId,
                    SystemRoleId = RoleType.TEACHER,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(RoleType.TEACHER);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("teacher", teacher);
                result.Add("teacherAccount", sysUser);
                result.Add("role", role);

                await _db.SaveChangesAsync();
                return new ServiceResponse()
                {
                    Success = true,
                    Data = result,
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }

        public static async Task<ServiceResponse> StudentCreate(AddStudentRequest studentReq, string password)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (string.IsNullOrEmpty(studentReq.StudentName))
                {
                    return ErrorHandler.BadRequestResponse(Message.StudentNameEmpty);
                }
                if (string.IsNullOrEmpty(studentReq.StudentPhone))
                {
                    return ErrorHandler.BadRequestResponse(Message.StudentPhoneEmpty);
                }
                var student_result = _db.Student
                    .Where(item => item.StudentPhone.Equals(studentReq.StudentPhone.Trim()))
                    .FirstOrDefault();
                if (student_result == null)
                {
                    student_result = new Student()
                    {
                        StudentId = Guid.NewGuid(),
                        StudentName = studentReq.StudentName.Trim(),
                        StudentPhone = studentReq.StudentPhone.Trim(),
                    };
                    _db.Student.Add(student_result);
                }

                var paramPhone = new SqlParameter("@studentPhone", studentReq.StudentPhone.Trim());
                var account_detail = await _db.UserDetail
                    .FromSqlRaw(StudentCheckAccount, paramPhone)
                    .FirstOrDefaultAsync();

                if (account_detail != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.UserAccountExist);
                }

                SystemUser sysUser = new SystemUser
                {
                    SystemUserId = Guid.NewGuid(),
                    Username = student_result.StudentName,
                    UserUsername = student_result.StudentPhone,
                    UserPassword = (string.IsNullOrEmpty(password)) ? Helper.EncodeMD5(student_result.StudentPhone) : Helper.EncodeMD5(password),
                };
                _db.SystemUser.Add(sysUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = student_result.StudentId,
                    SystemRoleId = RoleType.STUDENT,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(RoleType.STUDENT);

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("student", student_result);
                result.Add("studentAccount", sysUser);
                result.Add("role", role);
                await _db.SaveChangesAsync();

                return new ServiceResponse()
                {
                    Success = true,
                    Data = result,
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (Exception e)
            {
                return ErrorHandler.ErrorCatchResponse(e);
            }
        }
    }
}
