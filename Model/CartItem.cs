using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class CartItem
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CartId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? OrderId { get; set; }
        public string Sku { get; set; }
        public double? Price { get; set; }
        public double? Discount { get; set; }
        public int? Quantity { get; set; }
        public int? Active { get; set; }
        public string Content { get; set; }
        public string Code { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
