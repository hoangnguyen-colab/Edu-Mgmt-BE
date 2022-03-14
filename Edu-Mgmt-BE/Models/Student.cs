using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Student
    {
        public Student()
        {
            ClassDetail = new HashSet<ClassDetail>();
        }

        public Guid StudentId { get; set; }
        public string ShowStudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentGender { get; set; }
        public string StudentDOB { get; set; }
        public string StudentImage { get; set; }
        public string StudentDescription { get; set; }
        public string StudentPhone { get; set; }
        public string StudentAddress { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<ClassDetail> ClassDetail { get; set; }
    }
}
