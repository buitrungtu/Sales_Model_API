using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Sales_Model.Common;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sales_Model
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string key;
        public JwtAuthenticationManager(string key)
        {
            this.key = key;
        }
        public ServiceResponse LoginAuthenticate(Sales_ModelContext _db, string username, string password)
        {

            ServiceResponse res = new ServiceResponse();
            string passwordMD5 = this.EncodeMD5(password);
            var accountResult = _db.Accounts.Where(_ => _.Username == username && _.Password == passwordMD5).FirstOrDefault();
            if (accountResult == null)
            {
                res.Message = "Thông tin đăng nhập không chính xác!";
                res.Success = true;
                res.Data = null;
                res.ErrorCode = 404;
                return res;
            }
            accountResult.LastLogin = DateTime.Now;
            _db.Entry(accountResult).State = EntityState.Modified;
            Dictionary<string, object> result = new Dictionary<string, object>();
            //Cache lại thông tin tài khoản vừa đăng nhập
            //string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            //accountResult.LastIp = ipAddress;
            string sql_get_role = $"select * from role where role_id in (select distinct role_id from account_role where account_id = '{accountResult.AccountId}')";
            var roles = _db.Roles.FromSqlRaw(sql_get_role).ToList();
            result.Add("account", accountResult);
            result.Add("roles", roles);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(key);
            var tokenDesciptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,JsonConvert.SerializeObject(result))
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDesciptor);
            result.Add("token", tokenHandler.WriteToken(token));
            res.Success = true;
            res.Data = result;
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
