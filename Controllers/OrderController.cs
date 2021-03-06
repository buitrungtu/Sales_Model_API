using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Common;
using Sales_Model.Constants;
using Sales_Model.Model;
using Sales_Model.Model.ModelCustom;
using Sales_Model.Model.ModelCustom.Order;
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
        public async Task<ActionResult<PagingData>> GetOrderList([FromQuery] Guid? customerId,
            [FromQuery] string search,
            [FromQuery] string sort,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 20)
        {
            var pagingData = new PagingData();
            List<Order> records = new List<Order>();
            records = await _db.Orders.ToListAsync();
            List<OrderResponseFull> customerOrder = new List<OrderResponseFull>();

            if (!string.IsNullOrEmpty(customerId.ToString()))
            {
                records = await _db.Orders.Where(_ => _.AccountId.Equals(customerId.ToString())).ToListAsync();
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                string sql_get_order = "select * from orders where CHARINDEX(@txtSeach, convert(nvarchar(50), orders_id)) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.Orders.FromSqlRaw(sql_get_order, param).OrderByDescending(x => x.CreateDate).ToList();
            }
            if (!string.IsNullOrEmpty(sort))
            {
                records = Helper.OrderBy<Order>(records, sort).ToList();
            }

            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trang
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
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
            //var userInfo = _cache.Get("account_info");
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                res.Message = Message.OrderNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            var orderItem = await _db.OrdersItems.Where(_ => _.OrderId == order.OrdersId).ToListAsync();

            OrderResponseFull orderRes = new OrderResponseFull
            {
                items = orderItem,
                orderId = order.OrdersId.ToString(),
                customerName = order.CustomerName,
                customerPhone = order.CustomerPhone,
                customerAddress = order.CustomerAddress,
                Status = order.Status,
                SubTotal = order.SubTotal,
                ItemDiscount = order.ItemDiscount,
                Tax = order.Tax,
                Shipping = order.Shipping,
                Total = order.Total,
                Promo = order.Promo,
                Discount = order.Discount,
                GrandTotal = order.GrandTotal,
                Content = order.Content,
            };
            res.Data = orderRes;
            res.Success = true;
            return res;
        }

        /// <summary>
        /// Thêm order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/orders
        [HttpPost]
        public async Task<ServiceResponse> AddOrder(OrderInfoRequest requests)
        {
            ServiceResponse res = new ServiceResponse();
            //if (!Helper.CheckPermission(HttpContext, "AddOrder"))
            //{
            //    res.Success = false;
            //    res.Message = Message.NotAuthorize;
            //    res.ErrorCode = 403;
            //    return res;
            //}
            try
            {
                Order order = new Order();
                order.OrdersId = Guid.NewGuid();
                order.SessionId = "";
                order.Token = "";
                order.AccountId = requests.customerId;
                order.CustomerName = requests.customerName.Trim();
                order.CustomerPhone = requests.customerPhone.Trim();
                order.CustomerAddress = requests.customerAddress.Trim();
                order.Status = OrderStatus.Processing;
                order.CreateDate = DateTime.Now;
                order.UpdateDate = DateTime.Now;
                _db.Orders.Add(order);

                var orderItemList = new List<object>();
                double total = 0;

                foreach (var oi in requests.items)
                {
                    Product p = await _db.Products.FindAsync(oi.productId);
                    if (p == null)
                    {
                        res.Message = Message.ProductNotFound;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        return res;
                    }

                    int productExisted = (int)p.Quantity;
                    if (productExisted < oi.quantity)
                    {
                        res.Message = Message.QuantityNotEnough;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        return res;
                    }

                    if (p.Price == null)
                    {
                        res.Message = Message.ProductPriceNotFound;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        return res;
                    }

                    if (oi.quantity <= 0)
                    {
                        res.Message = Message.QuantityInvalid;
                        res.ErrorCode = 404;
                        res.Success = false;
                        res.Data = null;
                        return res;
                    }

                    OrdersItem orderItem = new OrdersItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.OrdersId,
                        ProductName = p.Title,
                        ProductImage = p.ProductImage,
                        Quantity = oi.quantity,
                        ProductId = p.ProductId,
                        Price = (double)(p.Discount != null ? (p.Price - p.Discount) * oi.quantity : p.Price * oi.quantity),
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                    };

                    _db.OrdersItems.Add(orderItem);

                    total += (double)(p.Discount != null ? (p.Price - p.Discount) * oi.quantity : p.Price * oi.quantity);
                    order.Total = total;

                    orderItemList.Add(oi);

                    p.Quantity = p.Quantity - oi.quantity;
                }

                await _db.SaveChangesAsync();

                OrderResponse orderRes = new OrderResponse
                {
                    items = orderItemList,
                    orderId = order.OrdersId.ToString(),
                    Total = order.Total,
                    Status = order.Status,
                    customerName = requests.customerName,
                    customerPhone = requests.customerPhone,
                    customerAddress = requests.customerAddress
                };

                Helper.WriteLogAsync(HttpContext, Message.OrderLogAdd);
                res.Success = true;
                res.Data = orderRes;
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
            }
            return res;
        }

        /// <summary>
        /// Xóa 1 order
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/order/7e8dbefb-74e6-46c5-9386-302008af7fb3
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteOrder(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "DeleteOrder"))
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                return res;
            }
            try
            {
                var order = await _db.Orders.FindAsync(id);
                if (order == null)
                {
                    res.Message = Message.ProductNotExist;
                    res.ErrorCode = 404;
                    res.Success = false;
                }

                _db.Orders.Remove(order);
                //delete ở các bảng liên quan 
                var lstOrderItems = _db.OrdersItems.Where(_ => _.OrderId == order.OrdersId);
                _db.OrdersItems.RemoveRange(lstOrderItems);
                await _db.SaveChangesAsync();

                Helper.WriteLogAsync(HttpContext, Message.OrderLogDelete);
                res.Success = false;
                res.Message = Message.OrderLogDeleteSuccess;
            }
            catch
            {
                res.Success = false;
                res.ErrorCode = 500;
                res.Message = Message.ErrorMsg;
            }
            return res;
        }

        /// <summary>
        /// Sửa trạng thái order: 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/order
        [HttpPost("change/status")]
        public async Task<ServiceResponse> ChangeOrderStatus(OrderInfoRequest req)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "ChangeOrder"))//Check quyền xóa
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                return res;
            }
            try
            {
                var order = await _db.Orders.FindAsync(req.orderId);
                if (order == null)
                {
                    res.Message = Message.OrderNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                }
                if (req.status >= OrderStatus.Processing && req.status <= OrderStatus.Error)
                {
                    order.Status = req.status;
                    order.CustomerAddress = req.customerAddress.Trim();
                    order.CustomerName = req.customerName.Trim();
                    order.CustomerPhone = req.customerPhone.Trim();
                    res.Data = order;
                    res.Message = Message.OrderStatusChanged;
                    res.Success = true;
                }
                else
                {
                    res.Message = Message.OrderStatusInvalid;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                }

                await _db.SaveChangesAsync();
                await Helper.WriteLogAsync(HttpContext, Message.OrderStatusChanged);

            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.Message = Message.ErrorMsg;
                res.ErrorCode = 500;
            }
            //}
            return res;
        }

        /// <summary>
        /// Huỷ order: 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/order
        [HttpPost("cancel")]
        public async Task<ServiceResponse> CanceledStatus(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var order = await _db.Orders.FindAsync(id);
                if (order == null)
                {
                    res.Message = Message.OrderNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                }
                order.Status = OrderStatus.Canceled;
                await _db.SaveChangesAsync();
                await Helper.WriteLogAsync(HttpContext, Message.OrderStatusChanged);
                res.Data = null;
                res.Message = Message.OrderCanceled;
                res.Success = true;
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
