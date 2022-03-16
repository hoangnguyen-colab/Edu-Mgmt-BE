using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Student;
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
                    SystemRoleId = 2,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(2);

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
                if (student_result != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.TeacherPhoneExist);
                }
                Student student = new Student()
                {
                    StudentId = Guid.NewGuid(),
                    StudentName = studentReq.StudentName.Trim(),
                    StudentGender = studentReq.StudentGender?.Trim() ?? "",
                    StudentImage = studentReq.StudentImage?.Trim() ?? "",
                    StudentDob = studentReq.StudentDob?.Trim() ?? "",
                    StudentPhone = studentReq.StudentPhone.Trim(),
                };
                _db.Student.Add(student);

                ClassDetail classDetail = null;
                if (!string.IsNullOrEmpty(studentReq.ClassId.ToString()))
                {
                    var classResult = await _db.Class.FindAsync(studentReq.ClassId);
                    if (classResult == null)
                    {
                        return ErrorHandler.NotFoundResponse(Message.ClassNotFound);
                    }
                    classDetail = new ClassDetail()
                    {
                        ClassDetailId = Guid.NewGuid(),
                        StudentId = student.StudentId,
                        ClassId = studentReq.ClassId.Value,
                    };
                    _db.ClassDetail.Add(classDetail);
                }

                SystemUser sysUser = new SystemUser
                {
                    SystemUserId = Guid.NewGuid(),
                    Username = student.StudentName,
                    UserUsername = student.StudentPhone,
                    UserPassword = (string.IsNullOrEmpty(password)) ? Helper.EncodeMD5(student.StudentPhone) : Helper.EncodeMD5(password),
                };
                _db.SystemUser.Add(sysUser);

                UserDetail sysUserDetail = new UserDetail
                {
                    UserDetailId = Guid.NewGuid(),
                    UserId = student.StudentId,
                    SystemRoleId = 3,
                    SystemUserId = sysUser.SystemUserId,
                };
                _db.UserDetail.Add(sysUserDetail);

                var role = await _db.SystemRole.FindAsync(3);

                if (classDetail != null)
                {
                    classDetail.StudentId = student.StudentId;
                }

                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("student", student);
                result.Add("studentAccount", sysUser);
                result.Add("role", role);
                result.Add("classDetail", classDetail);
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
