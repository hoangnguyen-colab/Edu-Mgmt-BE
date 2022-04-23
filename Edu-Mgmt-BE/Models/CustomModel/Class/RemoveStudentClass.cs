using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Class
{
    public class RemoveStudentClass
    {
        public Guid classId { get; set; }
        public Guid[] studentList { get; set; }
    }
}
