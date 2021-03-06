using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Sales_Model.Common
{
    public static class Helper
    {
        private static readonly Sales_ModelContext _db = new Sales_ModelContext();
        /// <summary>
        /// Ghi log vào db
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="_cache"></param>
        /// <param name="ip"></param>
        /// <param name="acction"></param>
        /// <returns></returns>
        /// @author bttu 11.6.2021
        public static async Task WriteLogAsync(HttpContext httpContext , string acction)
        {
            try
            {
                Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
                if (account_login != null && account_login.ContainsKey("account"))
                {
                    JObject jAccount = account_login["account"] as JObject;
                    Account account = jAccount.ToObject<Account>();
                    Auditinglog auditinglog = new Auditinglog();
                    auditinglog.AccountId = account.AccountId;
                    auditinglog.Action = acction;
                    auditinglog.Username = account.Username;
                    auditinglog.Ip = httpContext.Connection.RemoteIpAddress?.ToString();
                    await _db.Auditinglogs.AddAsync(auditinglog);
                    await _db.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                
            }
        }

        /// <summary>
        /// Check quyền
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="_cache"></param>
        /// <param name="ip"></param>
        /// <param name="acction"></param>
        /// <returns></returns>
        /// @author bttu 11.6.2021
        public static bool CheckPermission(HttpContext httpContext, string role_code)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                Account account = jAccount.ToObject<Account>();

                string sql_get_role = $"select * from role where role_id in (select distinct role_id from account_role where account_id = @account_id)";
                var roles = _db.Roles.FromSqlRaw(sql_get_role, new SqlParameter("@account_id", account.AccountId)).ToList();

                for (int i = 0 ; i < roles.Count(); i ++)
                {
                    if(roles[i].Role_Code == role_code || roles[i].Name == "Admin")
                    {
                        return true; // Nếu là admin hoặc có cái quyền này thì cho qua
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Lấy thông tin tài khoản hiện tại
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        /// @author bttu 11.6.2021
        public static Account GetUserInfo(HttpContext httpContext)
        {
            Dictionary<string, object> account_login = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpContext.User.Identity.Name);
            if (account_login != null && account_login.ContainsKey("account"))
            {
                JObject jAccount = account_login["account"] as JObject;
                Account account = jAccount.ToObject<Account>();
                return account;
            }
            return null;
        }
        /// <summary>
        /// Mã hóa MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// @author bttu 11.6.2021
        public static string EncodeMD5(string str)
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

        /// <summary>
        /// Sắp xếp theo thứ tự chuỗi truyền vào như sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> input, string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return input;

            int i = 0;
            foreach (string propname in queryString.Split(','))
            {
                var subContent = propname.Split(' ');
                if (subContent[1].Trim().ToLower() == "asc")
                {
                    if (i == 0)
                        input = input.OrderBy(x => GetPropertyValue(x, subContent[0].Trim()));
                    else
                        input = ((IOrderedEnumerable<T>)input).ThenBy(x => GetPropertyValue(x, subContent[0].Trim()));
                }
                else
                {
                    if (i == 0)
                        input = input.OrderByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
                    else
                        input = ((IOrderedEnumerable<T>)input).ThenByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
                }
                i++;
            }

            return input;
        }
        private static object GetPropertyValue(object obj, string property)
        {
            System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
            return propertyInfo.GetValue(obj, null);
        }
    }
}
