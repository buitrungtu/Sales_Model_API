using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Order
    {
        public Guid OrdersId { get; set; }
        public Guid? AccountId { get; set; }
        public string SessionId { get; set; }
        public string Token { get; set; }
        public int? Status { get; set; }
        public double? SubTotal { get; set; }
        public double? ItemDiscount { get; set; }
        public double? Tax { get; set; }
        public double? Shipping { get; set; }
        public double? Total { get; set; }
        public string Promo { get; set; }
        public double? Discount { get; set; }
        public double? GrandTotal { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
    }
}
