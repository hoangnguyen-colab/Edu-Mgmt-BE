using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Constants
{
    public enum RecordStatus
    {
        Delete = 0, // Xóa
        Edit = 1, // Sửa
        Add = 2 //Thêm

        //Scaffold-DbContext "Server=.\SQLExpress;Database=SchoolDB;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
    }
}
