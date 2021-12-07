using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Model.ModelCustom
{
    public class AccountInfoRequest
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
        public string EmailBackup { get; set; }
        public string DisplayName { get; set; }
        public int? IsInterestedAccount { get; set; }
        public List<int> RoleIds { get; set; }
    }
}
