using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class ClassDetail
    {
        public Guid ClassDetailId { get; set; }
        public Guid SchoolYearId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }

        public virtual Class Class { get; set; }
        public virtual SchoolYear SchoolYear { get; set; }
        public virtual Student Student { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}
