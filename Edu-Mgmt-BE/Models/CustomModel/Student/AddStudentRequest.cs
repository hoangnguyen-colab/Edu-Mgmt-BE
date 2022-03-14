using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Student
{
    public class AddStudentRequest
    {
        public Guid StudentId { get; set; }
        public string ShowStudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentGender { get; set; }
        public string StudentDOB { get; set; }
        public string StudentImage { get; set; }
        public string StudentDescription { get; set; }
        public string StudentPhone { get; set; }
        public string StudentAddress { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? SchoolYearId { get; set; }
    }
}
