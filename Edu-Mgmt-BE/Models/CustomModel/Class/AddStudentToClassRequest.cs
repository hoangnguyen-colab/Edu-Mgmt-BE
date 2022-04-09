using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Class
{
    public class AddStudentToClassRequest
    {
        public Guid classId { get; set; }
        public Models.Student[] studentList { get; set; }
    }
}
