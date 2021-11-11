using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Model.ModelCustom
{
    public class OrderInfoRequest
    {
        public List<OrderItemRequest> items;

        public string customerName { get; set; }

        public string customerPhone { get; set; }

        public string customerAddress { get; set; }
    }
}
