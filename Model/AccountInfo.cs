using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class AccountInfo
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string Profile { get; set; }
        public string Intro { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
