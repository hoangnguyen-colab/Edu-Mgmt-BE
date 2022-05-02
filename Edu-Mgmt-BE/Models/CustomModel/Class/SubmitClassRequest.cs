using System;
using System.Collections.Generic;

namespace Edu_Mgmt_BE.Models.CustomModel.Class
{
    public class SubmitClassRequest
    {
        public Guid studentId { get; set; }
        public List<Guid> classIds { get; set; }
        public string teacherPhone { get; set; }
        public string content { get; set; }
    }
}
