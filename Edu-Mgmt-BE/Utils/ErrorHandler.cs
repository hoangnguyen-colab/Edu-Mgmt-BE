using Edu_Mgmt_BE.Constants;
using Edu_Mgmt_BE.Model.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Utils
{
    public class ErrorHandler
    {
        public static ServiceResponse ErrorCatchResponse(Exception? exc, string errorMessage = Message.ErrorMsg)
        {
            ServiceResponse res = new ServiceResponse()
            {
                Message = errorMessage,
                Success = false,
                ErrorCode = 500,
                StatusCode = HttpStatusCode.InternalServerError,
                Data = exc,
            };
            return res;
        }

        public static ServiceResponse UnauthorizeCatchResponse()
        {
            ServiceResponse res = new ServiceResponse()
            {
                Data = null,
                Success = false,
                Message = Message.NotAuthorize,
                ErrorCode = 401,
                StatusCode = HttpStatusCode.Unauthorized,
            };
            return res;
        }

        public static ServiceResponse NotFoundResponse(string errorMessage = Message.NotFound)
        {
            ServiceResponse res = new ServiceResponse()
            {
                Data = null,
                Success = false,
                Message = errorMessage,
                ErrorCode = 404,
                StatusCode = HttpStatusCode.NotFound,
            };
            return res;
        }

        public static ServiceResponse BadRequestResponse(string errorMessage = Message.BadRequest)
        {
            ServiceResponse res = new ServiceResponse()
            {
                Data = null,
                Success = false,
                Message = errorMessage,
                ErrorCode = 400,
                StatusCode = HttpStatusCode.BadRequest,
            };
            return res;
        }
    }
}
