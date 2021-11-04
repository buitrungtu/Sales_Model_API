using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Sales_Model.Common;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;

        public AccountsController(Sales_ModelContext context, IMemoryCache memoryCache, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _db = context;
            _cache = memoryCache;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }
        // GET: api/<AccountsController>
        [HttpGet]
        public async Task<ServiceResponse> GetAccounts()
        {
            ServiceResponse res = new ServiceResponse();
            if (Helper.CheckPermission(HttpContext, "Admin")){
                res.Success = true;
                res.Data = await _db.Accounts.ToListAsync();
                return res;

            }
            res.Success = false;
            res.Message = "Bạn không có quyền này";
            res.ErrorCode = 403;
            return res;
        }
        /// <summary>
        /// Lấy chi tiết thông tin tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ServiceResponse> GetAccount(Account account)
        {
            return _jwtAuthenticationManager.LoginAuthenticate(_db, account.Username, account.Password);
        }
        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<ServiceResponse> Logout()
        {
            ServiceResponse res = new ServiceResponse();
            res.Success = true;
            res.Message = "Đăng xuất thành công";
            //Ghi log để làm nhật ký truy cập
            Helper.WriteLogAsync(_db, HttpContext, "Logout");
            return res;
        }

        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> PostAccount(Account account)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                account.AccountId = Guid.NewGuid();
                account.Password = this.EncodeMD5(account.Password);
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
                _db.Entry(account).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(_db, HttpContext ,"Thay đổi thông tin tài khoản");
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
        [HttpPost("edit_info")]
        public async Task<ServiceResponse> EditAccountInfo(Guid? id, AccountInfo accInfo)
        {
            ServiceResponse res = new ServiceResponse();
            _db.Entry(accInfo).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                Helper.WriteLogAsync(_db, HttpContext, "Sửa thông tin tài khoản chi tiết");
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
        public async Task<ServiceResponse> DeleteAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if(_cache.Get("user_" + ipAddress) == null) //Nếu không có thông tin tài khoản => thông báo đăng nhập lại
            {
                res.Success = false;
                res.Message = "Bạn vui lòng đăng nhập lại để thực hiện chức năng này";
                res.ErrorCode = 404;
                res.Data = "Not Found Info Account";
            }
            else if (!Helper.CheckPermission(HttpContext, "DeleteAccount"))//Check quyền xóa
            {
                res.Success = false;
                res.Message = "Bạn không có quyền thực hiện chức năng này";
                res.ErrorCode = 403;
                res.Data = "Not Permission";
            }
            else
            {
                var account = await _db.Accounts.FindAsync(id);
                if (account == null)
                {
                    res.Success = false;
                    res.Message = "Không tìm thấy tài khoản";
                    res.ErrorCode = 404;
                }
                AccountInfo accInfo = _db.AccountInfos.Where(_ => _.AccountId == account.AccountId).FirstOrDefault();
                _db.Accounts.Remove(account);
                _db.AccountInfos.Remove(accInfo);
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(_db, HttpContext, "Xóa tài khoản");
                res.Data = account;
                res.Success = true;
            }
            return res;
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
