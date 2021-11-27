using System;
using System.Collections.Generic;

namespace Sales_Model.Model.ModelCustom.Order
{
    public class OrderResponse
    {
        public List<object> items { get; set; }

        public string orderId { get; set; }

        public string customerName { get; set; }

        public string customerPhone { get; set; }

        public string customerAddress { get; set; }

        public string customerEmail { get; set; }
    }

    public class OrderResponseFull
    {
        public List<OutputDirectory.OrdersItem> items { get; set; }

        public string orderId { get; set; }

        public string customerName { get; set; }

        public string customerPhone { get; set; }

        public string customerAddress { get; set; }

        public string customerEmail { get; set; }
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
    }
}
