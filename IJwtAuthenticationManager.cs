using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model
{
    public interface IJwtAuthenticationManager
    {
        ServiceResponse LoginAuthenticate(Sales_ModelContext _db, string username, string password);
    }
}
