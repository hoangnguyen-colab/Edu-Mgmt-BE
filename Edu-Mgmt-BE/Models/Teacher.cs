using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Teacher
    {
        public Teacher()
        {
            ClassDetail = new HashSet<ClassDetail>();
        }

        public Guid TeacherId { get; set; }
        public string ShowTeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherImage { get; set; }
        public string TeacherDescription { get; set; }
        public string TeacherEmail { get; set; }
        public string TeacherPhone { get; set; }
        public string TeacherAddress { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<ClassDetail> ClassDetail { get; set; }
    }
}
