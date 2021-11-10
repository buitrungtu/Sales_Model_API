using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Constants
{
    public class OrderStatus
    {
        public const int Processing = 1;
        public const int Delivering = 2;
        public const int Received = 3;
        public const int Canceled = 4;
        public const int Return = 5;
        public const int Error = 6;
    }
}
