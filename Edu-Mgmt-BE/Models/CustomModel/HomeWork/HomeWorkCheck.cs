using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.HomeWork
{
    public class HomeWorkCheck
    {
        public Guid ClassId { get; set; }
        public Guid HomeWorkId { get; set; }
        public string StudentName { get; set; }
        public string StudentDob { get; set; }
        public string StudentPhone { get; set; }
    }
}
