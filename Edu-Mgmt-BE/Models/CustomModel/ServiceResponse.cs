using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Model.CustomModel
{
    public class ServiceResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; } = true;
        public object Data { get; set; }
        public int ErrorCode { get; set; }
    }
}
