using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class EditResultRequest
    {
        public Guid EditResultRequestId { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestContent { get; set; }
        public int? RequestStatus { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? ResultId { get; set; }

        public virtual Result Result { get; set; }
        public virtual Student Student { get; set; }
    }
}
