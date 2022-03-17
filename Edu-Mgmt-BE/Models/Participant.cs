using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Participant
    {
        public Guid ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantPhone { get; set; }
    }
}
