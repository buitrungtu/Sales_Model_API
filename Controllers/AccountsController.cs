using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sales_Model.Common;
using Sales_Model.Constants;
using Sales_Model.Model;
using Sales_Model.Model.ModelCustom;
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
    public class AccountsController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly Sales_ModelContext _db;

        public AccountsController(Sales_ModelContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }
        /// <summary>
        /// Lấy danh sách tài khoản có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/accounts?page=2&record=10&search=admin
        [HttpGet]
        public async Task<ServiceResponse> GetAccountsByPagingAndSearch([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 10)
        {
            ServiceResponse res = new ServiceResponse();
            if (Helper.CheckPermission(HttpContext, "Admin"))
            {
                var pagingData = new PagingData();
                List<Account> records = new List<Account>();
                //Tổng số bản ghi
                if (search != null && search.Trim() != "")
                {
                    //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                    string sql_get_account = "select * from account where CHARINDEX(@txtSeach, username) > 0 or CHARINDEX(@txtSeach, first_name) > 0" +
                                                                        "or CHARINDEX(@txtSeach, last_name) > 0 or CHARINDEX(@txtSeach, display_name) > 0" +
                                                                        "or CHARINDEX(@txtSeach, address) > 0 or CHARINDEX(@txtSeach, mobile) > 0";
                    var param = new SqlParameter("@txtSeach", search);
                    records = _db.Accounts.FromSqlRaw(sql_get_account, param).OrderByDescending(x => x.CreateDate).ToList();
                }
                else
                {
                    records = await _db.Accounts.OrderByDescending(x => x.CreateDate).ToListAsync();
                }
                pagingData.TotalRecord = records.Count(); //Tổng số bản ghi
                pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value)); //Tổng số trang
                pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(); //Dữ liệu của từng trang
                res.Success = true;
                res.Data = pagingData;
                return res;
            }
            res.Success = false;
            res.Message = Message.NotAuthorize;
            res.ErrorCode = 403;
            return res;
        }
        /// <summary>
        /// Lấy chi tiết thông tin tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ServiceResponse> GetAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
            {
                res.Data = null;
                res.Message = Message.AccountNotFound;
                res.ErrorCode = 404;
                return res;
            }
            //var account_info = _db.AccountInfos.Where(_ => _.AccountId == account.AccountId).FirstOrDefault();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("account", account);
            //result.Add("account_info", account_info);
            if (Helper.CheckPermission(HttpContext, "Admin"))//Nếu là admin thì trả về role của tk đó
            {
                string sql_get_role = $"select * from role where role_id in (select distinct role_id from account_role where account_id = @account_id)";
                var roles = _db.Roles.FromSqlRaw(sql_get_role, new SqlParameter("@account_id", id)).ToList();
                result.Add("roles", roles);
            }
            res.Data = result;
            res.Success = true;
            return res;
        }
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ServiceResponse> GetAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            res.Data = _jwtAuthenticationManager.LoginAuthenticate(_db, account.Username, account.Password).Data;
            if (res.Data != null)
            {
                //Ghi log để làm nhật ký truy cập
                //Auditinglog auditinglog = new Auditinglog();
                //auditinglog.Action = Message.AccountLogLogin;
                //auditinglog.Username = account.Username;
                //_db.Auditinglogs.Add(auditinglog);
                //await _db.SaveChangesAsync();
            }
            return res;
        }
        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<ServiceResponse> Logout()
        {
            ServiceResponse res = new ServiceResponse();
            res.Success = true;
            res.Message = Message.AccountLogoutSuccess;
            //Ghi log để làm nhật ký truy cập
            Helper.WriteLogAsync(HttpContext, Message.AccountLogLogout);
            return res;
        }

        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> PostAccount(AccountInfoRequest request)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                Account account = new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = request.Username.Trim(),
                    Password = Helper.EncodeMD5(request.Password.Trim()),
                    Address = request.Address,
                    Mobile = request.Mobile,
                    CreateDate = DateTime.Now,
                    DisplayName = request.DisplayName,
                    Status = AccountStatus.Active
                };
                
                _db.Accounts.Add(account);
                res.Success = true;
                res.Data = account;
                AccountInfo accInfo = new AccountInfo();
                accInfo.AccountId = request.AccountId;
                accInfo.Mobile = request.Mobile;
                if (request.Username.Contains("@gmail.com"))
                {
                    accInfo.Email = request.Username;
                }
                if (request.RoleIds != null && request.RoleIds.Count > 0)
                {
                    var listRole = new List<AccountRole>();
                    foreach (var roleId in request.RoleIds)
                    {
                        var accRole = new AccountRole
                        {
                            AccountId = account.AccountId,
                            RoleId = roleId,
                        };
                        listRole.Add(accRole);
                    }
                    _db.AccountRoles.AddRange(listRole);
                }
                _db.AccountInfos.Add(accInfo);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Message = Message.ErrorMsg;
                res.Success = false;
                res.ErrorCode = 500;
            }
            return res;
        }
        /// <summary>
        /// Sửa tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("edit_account")]
        public async Task<ServiceResponse> PutAccount(AccountInfoRequest request)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var accountDb = _db.Accounts.SingleOrDefault(_ => _.AccountId == request.AccountId);
                //Chỉ cập nhật những thông tin được phép thay đổi 
                accountDb.Avatar = request.Avatar != null ? request.Avatar : accountDb.Avatar;
                accountDb.Status = request.Status != null ? request.Status : accountDb.Status;
                accountDb.Address = request.Address != null ? request.Address : accountDb.Address;
                accountDb.Dob = request.Dob != null ? request.Dob : accountDb.Dob;
                accountDb.FirstName = request.FirstName != null ? request.FirstName : accountDb.FirstName;
                accountDb.LastName = request.LastName != null ? request.LastName : accountDb.LastName;
                accountDb.Mobile = request.Mobile != null ? request.Mobile : accountDb.Mobile;
                accountDb.EmailBackup = request.EmailBackup != null ? request.EmailBackup : accountDb.EmailBackup;
                accountDb.DisplayName = request.DisplayName != null ? request.DisplayName : accountDb.DisplayName;
                accountDb.IsInterestedAccount = request.IsInterestedAccount != null ? request.IsInterestedAccount : accountDb.IsInterestedAccount;
                // remove all old role before update new role of account
                var listOldRole = await _db.AccountRoles.Where(_ => _.AccountId == request.AccountId).ToListAsync();
                _db.AccountRoles.RemoveRange(listOldRole);
                if (request.RoleIds != null && request.RoleIds.Count > 0)
                {
                    var listRole = new List<AccountRole>();
                    foreach (var roleId in request.RoleIds)
                    {
                        var accRole = new AccountRole
                        {
                            AccountId = request.AccountId,
                            RoleId = roleId,
                        };
                        listRole.Add(accRole);
                    }
                    _db.AccountRoles.AddRange(listRole);
                }
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(HttpContext, Message.AccountLogChange);
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
                return res;
            }
            return res;
        }

        /// <summary>
        /// Lấy ds role của account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("role_of_account")]
        [AllowAnonymous]
        public async Task<ServiceResponse> GetRoleAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var list = await _db.AccountRoles.Where(_ => _.AccountId == id).ToListAsync();
                res.Data = list;
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
                return res;
            }
            return res;
        }
        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("edit_password")]
        public async Task<ServiceResponse> EditPasswordAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                Account accountDb = new Account();
                accountDb = _db.Accounts.SingleOrDefault(_ => _.AccountId == account.AccountId);
                if (accountDb == null)
                {
                    accountDb = _db.Accounts.SingleOrDefault(_ => _.Username == account.Username);
                }
                accountDb.Password = Helper.EncodeMD5(account.Password);
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(HttpContext, Message.AccountLogPassword);
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
                return res;
            }
            return res;
        }
        
        // DELETE api/<AccountsController>/5
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "DeleteAccount"))//Check quyền xóa
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
            }
            else
            {
                var account = await _db.Accounts.FindAsync(id);
                if (account == null)
                {
                    res.Success = false;
                    res.Message = Message.AccountNotFound;
                    res.ErrorCode = 404;
                }
                AccountInfo accInfo = _db.AccountInfos.Where(_ => _.AccountId == account.AccountId).FirstOrDefault();
                _db.Accounts.Remove(account);
                _db.AccountInfos.Remove(accInfo);
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(HttpContext, Message.AccountLogDelete);
                res.Data = account;
                res.Success = true;
            }
            return res;
        }
    }
}
