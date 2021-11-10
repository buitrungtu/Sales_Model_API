﻿using Microsoft.AspNetCore.Authorization;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales_Model.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;
        public OrderController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;
        }
        /// <summary>
        /// Lấy danh sách order có phân trang
        /// </summary>
        /// <param name="page">trang</param>
        /// <param name="record">số bản ghi trên 1 trang</param>
        /// <returns></returns>
        /// https://localhost:44335/api/order?page=2&record=10&search=mô+hình
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PagingData>> GetOrderList([FromQuery] int page, [FromQuery] int record, [FromQuery] string search)
        {
            var pagingData = new PagingData();
            List<Order> records = new List<Order>();
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trang
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page - 1) * record).Take(record).ToList();
            return pagingData;
        }

        /// <summary>
        /// Lấy thông tin chi tiết order theo id
        /// </summary>
        /// <param name="id">id của order</param>
        /// <returns></returns>
        /// https://localhost:44335/api/order/detail?id=7e8dbefb-74e6-46c5-9386-302008af7fb3
        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<ServiceResponse> GetOrderDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var userInfo = _cache.Get("account_info");
            var product = await _db.Orders.FindAsync(id);
            if (product == null)
            {
                res.Message = Message.ProductNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("product", product);
            res.Data = result;
            res.Success = true;
            return res;
        }

        /// <summary>
        /// Thêm product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products
        [HttpPost]
        public async Task<ServiceResponse> PostProduct(Order order)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                order.OrdersId = Guid.NewGuid();
                order.SessionId = "";
                order.Token = "";
                order.Status = OrderStatus.Processing;
                order.CreateDate = DateTime.Now;
                order.UpdateDate = DateTime.Now;
                //_db.Orders.Add(order);
                //await _db.SaveChangesAsync();
                Helper.WriteLogAsync(_db, HttpContext, Message.OrderLogAdd);
                res.Success = true;
                res.Data = _db.Orders.Where(_ => _.OrdersId == order.OrdersId).FirstOrDefault();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
            }
            return res;
        }
    }
}