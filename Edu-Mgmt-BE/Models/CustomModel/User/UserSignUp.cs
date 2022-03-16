using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.User
{
    public class UserSignUp
    {
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string UserPassword { get; set; }
        public string UserEmail { get; set; }
        public int RoleId { get; set; }
    }
}
