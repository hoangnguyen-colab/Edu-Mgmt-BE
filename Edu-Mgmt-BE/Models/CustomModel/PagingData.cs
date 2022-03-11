using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Model.CustomModel
{
    public class PagingData
    {
        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPage { get; set; }
        /// <summary>
        /// Tổng số bản ghi
        /// </summary>
        public int TotalRecord { get; set; }
        /// <summary>
        /// Dữ liệu trong 1 trang
        /// </summary>
        public object Data { get; set; }
    }
}
