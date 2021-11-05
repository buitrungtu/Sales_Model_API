using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class AccountRole
    {
        public Guid? AccountId { get; set; }
        public int? RoleId { get; set; }

        [NotMapped]
        public int State { get; set; }
        [NotMapped]
        public string Username { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [NotMapped]
        public string Role_code { get; set; }
        [NotMapped]
        public string Name { get; set; }
    }
}
