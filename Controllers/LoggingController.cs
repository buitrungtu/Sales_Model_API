using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

namespace Sales_Model.Controllers
{
    //admin mới có quyền xem log
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;

        public LoggingController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;

        }
        /// <summary>
        /// Lấy full thông tin log action
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ServiceResponse> GetAccounts()
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "Admin"))//Check quyền
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
                return res;
            }
            res.Data = await _db.Auditinglogs.ToListAsync();
            res.Success = true;
            return res;
        }

        /// <summary>
        /// Lấy danh sách log có phân trang
        /// </summary>
        /// <param name="page">trang</param>
        /// <param name="record">số bản ghi trên 1 trang</param>
        /// <returns></returns>
        [HttpGet("paging")]
        public async Task<ServiceResponse> GetLogPaging([FromQuery] int page, [FromQuery] int record)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "Admin"))//Check quyền
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
                return res;
            }
            var pagingData = new PagingData();
            //Tổng số bản ghi
            var records = await _db.Auditinglogs.OrderByDescending(x => x.CreateDate).ToListAsync();
            pagingData.TotalRecord = records.Count();
            //Tổng số trang
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page - 1) * record).Take(record).ToList();
            res.Data = pagingData;
            res.Success = true;
            return res;
        }
    }
}
