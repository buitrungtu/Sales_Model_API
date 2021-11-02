using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class ProductReview
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ParentId { get; set; }
        public string Title { get; set; }
        public int? Rating { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? PublishedDate { get; set; }

        [NotMapped]
        public int State { get; set; } // 0 - Xóa, 1 - Sửa, 2 - Thêm
    }
}
