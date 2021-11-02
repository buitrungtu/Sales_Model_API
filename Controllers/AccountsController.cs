using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales_Model.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;

        public AccountsController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;

        }
        // GET: api/<AccountsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _db.Accounts.ToListAsync();
        }

        // GET api/<AccountsController>/5
        [HttpGet("{id}")]
        public async Task<ServiceResponse> GetAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
            {
                res.Data = null;
                res.Message = "Không tìm thấy tài khoản này";
                res.ErrorCode = 404;
                return res;
            }
            var account_info = _db.AccountInfos.Where(_=>_.AccountId == account.AccountId).FirstOrDefault();
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("account", account);
            result.Add("account_info", account_info);
            res.Data = result;
            res.Success = true;
            return res;
        }

        [HttpPost("login")]
        public async Task<ServiceResponse> GetAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();

            string password = this.EncodeMD5(account.Password);
            var accountResult = _db.Accounts.Where(_ => _.Username == account.Username && _.Password == password).FirstOrDefault();
            if (accountResult == null)
            {
                res.Message = "Thông tin đăng nhập không chính xác!";
                res.Success = true;
                res.Data = null;
                res.ErrorCode = 404;
                return res;
            }
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            accountResult.LastLogin = DateTime.Now;
            accountResult.LastIp = ipAddress;
            _db.Entry(accountResult).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            string cacheKey = "user_" + ipAddress;
            _cache.Set(cacheKey, accountResult);
            Dictionary<string, object> result = new Dictionary<string, object>();
            accountResult.Password = account.Password;
            result.Add("account", accountResult);
            string sql_get_role = $"select * from role where role_id in (select role_id from account_role where account_id = '{accountResult.AccountId}')";
            var roles = _db.Roles.FromSqlRaw(sql_get_role).ToList();
            result.Add("roles", roles);
            res.Success = true;
            res.Data = result;
            Auditinglog auditinglog = new Auditinglog();
            auditinglog.AccountId = accountResult.AccountId;

            //Ghi log
            //_db.Auditinglogs.Add()

            return res;
        }

        [HttpPost("logout")]
        public async Task<ServiceResponse> Logout()
        {
            ServiceResponse res = new ServiceResponse();
            res.Success = true;
            res.Message = "Đăng xuất thành công";
            _cache.Set("account_info", new object());
            return res;
        }

        [HttpPost]
        public async Task<ServiceResponse> PostAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _db.Accounts.Add(account);
                res.Success = true;
                Account result = _db.Accounts.Where(_ => _.Username == account.Username).FirstOrDefault();
                res.Data = result;
                AccountInfo accInfo = new AccountInfo();
                accInfo.AccountId = result.AccountId;
                if (result.Username.Contains("@gmail.com"))
                {
                    accInfo.Email = result.Username;
                }
                _db.AccountInfos.Add(accInfo);
                await _db.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.ErrorCode = 500; 
            }
            return res;
        }
        // PUT api/<AccountsController>/5
        [HttpPut("{id}")]
        public async Task<ServiceResponse> PutAccount(Guid? id, Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _db.Entry(account).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = "Có lỗi sảy ra";
                res.ErrorCode = 500;
                return res;
            }
            return res;
        }
        /// <summary>
        /// Sửa account_info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accInfo"></param>
        /// <returns></returns>
        [HttpPut("edit_info")]
        public async Task<ServiceResponse> EditAccountInfo(Guid? id, AccountInfo accInfo)
        {
            ServiceResponse res = new ServiceResponse();
            _db.Entry(accInfo).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = "Có lỗi sảy ra";
                res.ErrorCode = 500;
                return res;
            }
            return res;
        }
        // DELETE api/<AccountsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Account>> DeleteAccount(Guid? id)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            AccountInfo accInfo = _db.AccountInfos.Where(_ => _.AccountId == account.AccountId).FirstOrDefault();
            _db.Accounts.Remove(account);
            _db.AccountInfos.Remove(accInfo);
            await _db.SaveChangesAsync();

            return account;
        }

        /// <summary>
        /// Mã hóa MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string EncodeMD5(string str)
        {
            string result = "";
            if (str != null)
            {
                MD5 md = MD5.Create();
                byte[] bytePass = Encoding.ASCII.GetBytes(str);
                byte[] byteResult = md.ComputeHash(bytePass);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < byteResult.Length; i++)
                {
                    sb.Append(byteResult[i].ToString("X2"));
                }
                result = sb.ToString();
            }
            return result;
        }
    }
}
