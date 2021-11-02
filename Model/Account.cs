using System;
using System.Collections.Generic;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Account
    {
        public Guid AccountId { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? Status { get; set; }
        public string Address { get; set; }
        public DateTime? Dob { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastAction { get; set; }
        public string LastIp { get; set; }
        public string EmailBackup { get; set; }
        public string DisplayName { get; set; }
        public int? IsInterestedAccount { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
