using Edu_Mgmt_BE.Common;
using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Edu_Mgmt_BE.Models.CustomModel.Result;
using Edu_Mgmt_BE.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Controllers
{
    [Route("api/result")]
    [ApiController]
    public class ResultController : ControllerBase
    {

        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly EduManagementContext _db;

        public ResultController(EduManagementContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Chấm điểm bài nộp
        /// </summary>
        /// <param name="answerReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> SubmitResult(ResultSubmit req)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                if (req.AnswerId == null || req.AnswerId == Guid.Empty)
                {
                    return ErrorHandler.BadRequestResponse(Message.AnswerEmpty);
                }

                var answer_find = await _db.Answer.FindAsync(req.AnswerId);
                if (answer_find == null)
                {
                    return ErrorHandler.NotFoundResponse(Message.AnswerNotFound);
                }
                result.Add("answer", answer_find);

                double score;
                if (!double.TryParse(req.FinalScore, out score))
                {
                    return ErrorHandler.BadRequestResponse(Message.ResultScoreBadReq);
                }

                var teacher_id = Helper.getTeacherId(HttpContext);

                var result_check_exist = await _db.Result
                    .Where(x => x.AnswerId.Equals(answer_find.AnswerId))
                    .FirstOrDefaultAsync();
                if (result_check_exist != null)
                {
                    return ErrorHandler.BadRequestResponse(Message.ResultAlreadySubmit);
                }

                Result answer_result = new Result()
                {
                    ResultId = Guid.NewGuid(),
                    FinalScore = req.FinalScore,
                    CreatedDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    ResultContent = req.ResultContent,
                    AnswerId = answer_find.AnswerId,
                    TeacherId = teacher_id
                };
                result.Add("result", answer_result);
                _db.Result.Add(answer_result);
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

        /// <summary>
        /// Sửa điểm bài nộp
        /// </summary>
        /// <param name="answerReq"></param>
        /// <returns></returns>
        [HttpPut("edit/{id}")]
        public async Task<ServiceResponse> EditResult(Guid id, ResultSubmit req)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "admin") && !Helper.CheckPermission(HttpContext, "teacher"))
            {
                return ErrorHandler.UnauthorizeCatchResponse();
            }
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                var result_check = await _db.Result.FindAsync(id);
                if (result_check == null)
                {
                    return ErrorHandler.BadRequestResponse(Message.ResultNotFound);
                }
                double score;
                if (!double.TryParse(req.FinalScore, out score))
                {
                    return ErrorHandler.BadRequestResponse(Message.ResultScoreBadReq);
                }

                result_check.FinalScore = req.FinalScore;
                result_check.ModifyDate = DateTime.Now;
                result_check.ResultContent = req.ResultContent;

                //await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = result_check;
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
