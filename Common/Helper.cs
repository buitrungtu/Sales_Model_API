using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Model;
using Sales_Model.OutputDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sales_Model.Common
{
    public class Helper
    {
        /// <summary>
        /// Ghi log vào db
        /// </summary>
        /// <param name="_db"></param>
        /// <param name="_cache"></param>
        /// <param name="ip"></param>
        /// <param name="acction"></param>
        /// <returns></returns>
        public static async Task WriteLogAsync(Sales_ModelContext _db, IMemoryCache _cache , string ip, string acction)
        {
            string cacheKey = "user_" + ip;
            Dictionary<string, object> accountInfo = (Dictionary<string, object>)_cache.Get(cacheKey);
            if (accountInfo != null && accountInfo.ContainsKey("account"))
            { 
                Account account = (Account)accountInfo["account"];
                Auditinglog auditinglog = new Auditinglog();
                auditinglog.AccountId = account.AccountId;
                auditinglog.Action = acction;
                auditinglog.Username = account.Username;
                auditinglog.Ip = ip;
                _db.Auditinglogs.Add(auditinglog);
                await _db.SaveChangesAsync();
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
        public static bool CheckPermission(IMemoryCache _cache, string ip, string role_code)
        {
            string cacheKey = "user_" + ip;
            Dictionary<string, object> accountInfo = (Dictionary<string, object>)_cache.Get(cacheKey);
            if (accountInfo != null && accountInfo.ContainsKey("roles"))
            {
                List<Role> roles = (List<Role>)accountInfo["roles"];
                for(int i =0 ; i < roles.Count(); i ++)
                {
                    if(roles[i].Role_Code == role_code || roles[i].Name == "Admin")
                    {
                        return true; // Nếu là admin hoặc có cái quyền này thì cho qua
                    }
                }
            }
            return false;
        }
    }
}
