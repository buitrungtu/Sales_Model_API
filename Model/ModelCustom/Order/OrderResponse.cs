using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Model.ModelCustom.Order
{
    public class OrderResponse
    {
        public List<object> items;

        public string orderId { get; set; }

        public string customerName { get; set; }

        public string customerPhone { get; set; }

        public string customerAddress { get; set; }

        public string customerEmail { get; set; }
    }
}
