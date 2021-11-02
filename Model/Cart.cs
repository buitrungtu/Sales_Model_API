using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Cart
    {
        public Guid CartId { get; set; }
        public Guid? AccountId { get; set; }
        public string SessionId { get; set; }
        public string Token { get; set; }
        public int? Status { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
