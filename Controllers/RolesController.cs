using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Common;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales_Model.Controllers
{
    [Authorize]
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
        public async Task<ServiceResponse> GetRoles()
        {
            ServiceResponse res = new ServiceResponse();
            if (Helper.CheckPermission(HttpContext, "Admin"))
            {
                res.Success = true;
                res.Data = await _db.Roles.ToListAsync();
                return res;

            }
            res.Success = false;
            res.Message = "Bạn không có quyền này";
            res.ErrorCode = 403;
            return res;
        }
        // GET: api/<RolesController>
        [HttpGet("roles_account")]
        public async Task<ServiceResponse> GetRolesAccount()
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "Admin"))
            {
                res.Success = false;
                res.Message = "Bạn không có quyền này";
                return res;

            }
            string sql_get_role = $"select acc.account_id, username, status, r.role_id, r.role_code, r.name from account acc join account_role accr on acc.account_id = accr.account_id join role r on accr.role_id = r.role_id";
            res.Data = _db.AccountRoles.FromSqlRaw(sql_get_role).ToList();
            res.Success = false;
            res.Message = "Bạn không có quyền này";
            res.ErrorCode = 403;
            return res;
        }
        // PUT api/<RolesController>/5
        [HttpPost("edit_role")]
        public ServiceResponse EditRole(List<AccountRole> lstAccountRole)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "Admin"))
            {
                res.Success = false;
                res.Message = "Bạn không có quyền này";
                res.ErrorCode = 403;
                return res;
            }
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
