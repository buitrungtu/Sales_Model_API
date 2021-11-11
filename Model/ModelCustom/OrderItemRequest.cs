using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;

namespace Sales_Model.Model.ModelCustom
{
    public class OrderItemRequest
    {
        public Guid productId { get; set; }

        public int quantity { get; set; }


        public OrderItemRequest(Guid productId, int quantityr)
        {
            this.productId = productId;
            this.quantity = quantity;
        }

    }
}
