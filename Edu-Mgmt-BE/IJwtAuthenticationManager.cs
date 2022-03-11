using Edu_Mgmt_BE.Model.CustomModel;
using Edu_Mgmt_BE.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE
{
    public interface IJwtAuthenticationManager
    {
        ServiceResponse LoginAuthenticate(EduManagementContext _db, string username, string password);
    }
}
