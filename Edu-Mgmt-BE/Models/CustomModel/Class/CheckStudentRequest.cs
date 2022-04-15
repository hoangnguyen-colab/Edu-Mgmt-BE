using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Class
{
    public class CheckStudentById
    {
        public Guid classId { get; set; }
        public Guid? studentId { get; set; }
    }

    public class CheckStudentByField
    {
        public Guid classId { get; set; }
        public string studentName { get; set; }
        public string studentDob { get; set; }
        public string studentPhone { get; set; }
    }
}
