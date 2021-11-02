using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Model
{
    public class Auditinglog
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public Guid AccountId { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
