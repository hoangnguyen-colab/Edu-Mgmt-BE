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
            Class = new HashSet<Class>();
        }

        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherPhone { get; set; }
        public string TeacherGender { get; set; }
        public string TeacherDob { get; set; }
        public string TeacherImage { get; set; }
        public string TeacherEmail { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Class> Class { get; set; }
    }
}
