using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Transaction
    {
        public Guid TransactionId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? OrderId { get; set; }
        public string Code { get; set; }
        public int? Type { get; set; }
        public int? Mode { get; set; }
        public int? Status { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
