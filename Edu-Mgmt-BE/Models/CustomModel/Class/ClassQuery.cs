using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Class
{
    public class ClassQuery
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassYear { get; set; }
        public int ClassStatus { get; set; }
        public Guid TeacherId { get; set; }
        public int HomeWorkCount { get; set; }
    }

}
