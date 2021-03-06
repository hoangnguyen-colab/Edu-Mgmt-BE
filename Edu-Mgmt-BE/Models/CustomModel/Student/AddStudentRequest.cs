using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Student
{
    public class AddStudentRequest
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentGender { get; set; }
        public string StudentDob { get; set; }
        public string StudentPhone { get; set; }
        public Guid? ClassId { get; set; }
    }
}
