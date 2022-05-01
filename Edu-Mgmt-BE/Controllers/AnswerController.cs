using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.HomeWork;
using Edu_Mgmt_BE.Utils;
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
    [Route("api/answer")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        private const string query_get_answer_files = "SELECT DISTINCT FileUpload.* FROM AnswerFileDetail, FileUpload WHERE AnswerFileDetail.AnswerId = @answerId AND FileUpload.FileUploadId = AnswerFileDetail.FileUploadId";
        private const string query_get_class_by_homework = "SELECT DISTINCT Class.* FROM HomeWorkClassDetail, Class WHERE Class.ClassId = HomeWorkClassDetail.ClassId AND HomeWorkClassDetail.HomeWorkId = @homeWorkId";

        public AnswerController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Lấy danh sách bài nộp theo bài tập có phân trang
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/answer?page=2&record=10&search=21
        [HttpGet("by-homework/{HomeWorkId}")]
        public async Task<ServiceResponse> GetAnswerByHomeWork(
            Guid HomeWorkId,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var homework_result = await _db.HomeWork.FindAsync(HomeWorkId);
                if (homework_result == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.HomeWorkNotFound);
                }

                var pagingData = new PagingData();
                List<Answer> records = await _db.Answer
                    .Where(x => x.HomeWorkId.Equals(HomeWorkId))
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                foreach (var item in records)
                {
                    var student = await _db.Student.FindAsync(item.StudentId);
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
        /// Học sinh nộp bài tập
        /// </summary>
        /// <param name="answerReq"></param>
        /// <returns></returns>
        [HttpPost("submit")]
        public async Task<ServiceResponse> SubmitHomeWork(HomeWorkAnswer answerReq)
        {
            ServiceResponse res = new ServiceResponse();
            //if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            //{
            //    return ErrorHandler.UnauthorizeCatchResponse();
            //}
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                var class_result = await _db.Class.FindAsync(answerReq.ClassId);
                if (class_result == null)
                {
                    return ErrorHandler.BadRequestResponse(Message.ClassNotFound);
                }
                result.Add("class", class_result);

                var hw_result = await _db.HomeWork.FindAsync(answerReq.HomeWorkId);
                if (hw_result == null)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeWorkNotFound);
                }
                if (hw_result?.DueDate < DateTime.Now)
                {
                    return ErrorHandler.BadRequestResponse(Message.HomeDueDate);
                }

                Student student = null;
                if (answerReq.StudentId == null || answerReq.StudentId == Guid.Empty)
                {
                    student = new Student()
                    {
                        StudentId = Guid.NewGuid(),
                        StudentDob = answerReq.StudentDob.Trim(),
                        StudentName = answerReq.StudentName.Trim(),
                        StudentPhone = answerReq.StudentPhone.Trim(),
                    };
                    _db.Student.Add(student);
                }
                else
                {
                    student = await _db.Student.FindAsync(answerReq.StudentId);
                    if (student == null)
                    {
                        return ErrorHandler.BadRequestResponse(Message.StudentNotFound);
                    }

                    var student_submit_check = await _db.Answer
                        .Where(x => x.StudentId.Equals(student.StudentId)
                        && x.ClassId.Equals(class_result.ClassId)
                        && x.HomeWorkId.Equals(hw_result.HomeWorkId)
                        )
                        .FirstOrDefaultAsync();

                    if (student_submit_check != null)
                    {
                        return ErrorHandler.BadRequestResponse(Message.AnswerAlreadySubmit);
                    }
                }
                result.Add("student", student);

                Answer answer = new Answer()
                {
                    AnswerId = Guid.NewGuid(),
                    SubmitTime = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    AnswerContent = answerReq.AnswerContent,
                    ClassId = class_result.ClassId,
                    StudentId = student.StudentId,
                    HomeWorkId = answerReq.HomeWorkId,
                };
                //_db.Answer.Add(answer);
                result.Add("answer", answer);

                if (answerReq.FileList?.Length > 0)
                {
                    List<Models.FileUpload> fileList = new List<Models.FileUpload>();
                    List<AnswerFileDetail> fileListDetail = new List<AnswerFileDetail>();
                    foreach (var file in answerReq.FileList)
                    {
                        Models.FileUpload fileItem = new Models.FileUpload()
                        {
                            FileUploadId = Guid.NewGuid(),
                            FileUploadName = file.FileUploadName,
                            FileUploadUrl = file.FileUploadUrl,
                        };

                        fileList.Add(fileItem);
                        fileListDetail.Add(new AnswerFileDetail()
                        {
                            FileUploadDetailId = Guid.NewGuid(),
                            AnswerId = answer.AnswerId,
                            FileUploadId = fileItem.FileUploadId,
                        });
                    }
                    //_db.FileUpload.AddRange(fileList);
                    //_db.AnswerFileDetail.AddRange(fileListDetail);
                    result.Add("fileList", fileList);
                    //result.Add("fileListDetail", fileListDetail);
                }

                //await _db.SaveChangesAsync();

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

        /// <summary>
        /// Chi tiết bài nộp
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        [HttpGet("detail/{id}")]
        public async Task<ServiceResponse> GetAnswerDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var answer_result = await _db.Answer.FindAsync(id);
            if (answer_result == null)
            {
                return ErrorHandler.NotFoundResponse(Message.AnswerNotFound);
            }

            var param = new SqlParameter("@answerId", answer_result.AnswerId);
            var filesRecords = _db.FileUpload
                .FromSqlRaw(query_get_answer_files, param)
                .OrderByDescending(x => x.FileUploadName)
                .ToList();
            var student = await _db.Student.FindAsync(answer_result.StudentId);

            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("answer", answer_result);
            //result.Add("student", student);
            result.Add("files", filesRecords);

            res.Data = result;
            res.Success = true;
            res.StatusCode = HttpStatusCode.OK;
            return res;
        }

    }
}
