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
            Answer = new HashSet<Answer>();
            ClassRequest = new HashSet<ClassRequest>();
            EditResultRequest = new HashSet<EditResultRequest>();
        }

        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public string StudentDob { get; set; }
        public string StudentGender { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<Answer> Answer { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<ClassRequest> ClassRequest { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<EditResultRequest> EditResultRequest { get; set; }
    }
}
