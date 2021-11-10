using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Common;
using Sales_Model.Constants;
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
        /// <summary>
        /// Get roles by account_id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/roles/roles_account?id=0f76c9fa-509f-4e75-afde-2a79b5c9df56
        [HttpGet("roles_account")]
        public async Task<ServiceResponse> GetRolesAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "Admin"))//Check quyền admin
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
            }
            string sql_get_role = $"select * from role where role_id in (select distinct role_id from account_role where account_id = @account_id)";
            var roles = _db.Roles.FromSqlRaw(sql_get_role, new SqlParameter("@account_id", id)).ToList();
            res.Data = roles;
            res.Success = true;
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
                    if (item.State == null) item.State = 0;
                    switch (item.State)
                    {
                        case (int)RecordStatus.Delete:
                            lstDelete.Add(item);
                            break;
                        case (int)RecordStatus.Add:
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
