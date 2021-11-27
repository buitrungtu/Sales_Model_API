using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Model
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public string Url { get; set; }
        public string AltContent { get; set; }
        public string Content { get; set; }

        [NotMapped]
        public int State { get; set; } // 0 - Xóa, 1 - Sửa, 2 - Thêm
    }
}
