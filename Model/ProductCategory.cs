using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class ProductCategory
    {
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }

        [NotMapped]
        public int State { get; set; } // 0 - Xóa, 1 - Sửa, 2 - Thêm
    }
}
