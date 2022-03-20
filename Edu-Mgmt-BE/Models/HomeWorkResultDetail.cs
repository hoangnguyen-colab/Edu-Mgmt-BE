using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class HomeWorkResultDetail
    {
        public Guid HomeWorkResultDetailId { get; set; }
        public Guid ResultId { get; set; }
        public Guid? AnswerId { get; set; }
        public Guid? HomeWorkId { get; set; }

        public virtual Answer Answer { get; set; }
        public virtual HomeWork HomeWork { get; set; }
        public virtual Result Result { get; set; }
    }
}
