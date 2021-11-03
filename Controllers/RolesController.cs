using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales_Model.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;

        public RolesController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;

        }
        // GET: api/<RolesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _db.Roles.ToListAsync();
        }

        // PUT api/<RolesController>/5
        [HttpPut("{id}")]
        public ServiceResponse EditRole(List<AccountRole> lstAccountRole)
        {
            ServiceResponse res = new ServiceResponse();
            if(lstAccountRole != null && lstAccountRole.Count() > 0)
            {
                var lstDelete = new List<AccountRole>();
                var lstInsert = new List<AccountRole>();
                foreach (var item in lstAccountRole)
                {
                    switch (item.State)
                    {
                        case 0:
                            lstDelete.Add(item);
                            break;
                        case 2:
                            lstInsert.Add(item);
                            break;
                        default:
                            break;
                    }
                }
                if (lstDelete.Count > 0)
                {
                    _db.AccountRoles.RemoveRange(lstDelete);
                }
                if (lstInsert.Count > 0)
                {
                    _db.AccountRoles.AddRange(lstInsert);
                }
                res.Success = true;
            }
            else
            {
                res.Success = false;
            }
            return res;
        }
    }
}
