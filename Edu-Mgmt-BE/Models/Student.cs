using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Student
    {
        public Guid StudentId { get; set; }
        public string StudentPhone { get; set; }
        public string StudentName { get; set; }
        public string StudentGender { get; set; }
        public string StudentDob { get; set; }
        public string StudentImage { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }
    }
}
