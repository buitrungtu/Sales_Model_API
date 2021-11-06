using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class AccountsController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly Sales_ModelContext _db;

        public AccountsController(Sales_ModelContext context, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }
        // GET: api/<AccountsController>
        [HttpGet]
        public async Task<ServiceResponse> GetAccounts()
        {
            ServiceResponse res = new ServiceResponse();
            if (Helper.CheckPermission(HttpContext, "Admin"))
            {
                res.Success = true;
                res.Data = await _db.Accounts.ToListAsync();
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
            var account_info = _db.AccountInfos.Where(_ => _.AccountId == account.AccountId).FirstOrDefault();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("account", account);
            result.Add("account_info", account_info);
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
                Auditinglog auditinglog = new Auditinglog();
                auditinglog.Action = Message.AccountLogLogin;
                auditinglog.Username = account.Username;
                _db.Auditinglogs.Add(auditinglog);
                await _db.SaveChangesAsync();
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
            Helper.WriteLogAsync(_db, HttpContext, Message.AccountLogLogout);
            return res;
        }

        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ServiceResponse> PostAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                account.AccountId = Guid.NewGuid();
                account.Password = Helper.EncodeMD5(account.Password);
                _db.Accounts.Add(account);
                res.Success = true;
                res.Data = account;
                AccountInfo accInfo = new AccountInfo();
                accInfo.AccountId = account.AccountId;
                if (account.Username.Contains("@gmail.com"))
                {
                    accInfo.Email = account.Username;
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
        public async Task<ServiceResponse> PutAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var accountDb = _db.Accounts.SingleOrDefault(_ => _.AccountId == account.AccountId);
                //Chỉ cập nhật những thông tin được phép thay đổi
                accountDb.Avatar = account.Avatar;
                accountDb.Status = account.Status;
                accountDb.Address = account.Address;
                accountDb.Dob = account.Dob;
                accountDb.FirstName = account.FirstName;
                accountDb.LastName = account.LastName;
                accountDb.Mobile = account.Mobile;
                accountDb.EmailBackup = account.EmailBackup;
                accountDb.DisplayName = account.DisplayName;
                accountDb.IsInterestedAccount = account.IsInterestedAccount;
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(_db, HttpContext, Message.AccountLogChange);
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
                Helper.WriteLogAsync(_db, HttpContext, Message.AccountLogPassword);
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
                Helper.WriteLogAsync(_db, HttpContext, Message.AccountLogDelete);
                res.Data = account;
                res.Success = true;
            }
            return res;
        }
    }
}
