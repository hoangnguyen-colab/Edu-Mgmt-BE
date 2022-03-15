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
    }
}
