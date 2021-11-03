﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;
        public ProductsController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;
        }
        /// <summary>
        /// Lấy danh sách product có phân trang
        /// </summary>
        /// <param name="page">trang</param>
        /// <param name="record">số bản ghi trên 1 trang</param>
        /// <returns></returns>
        /// https://localhost:44335/api/products?page=2&&record=10
        [HttpGet]
        public async Task<ActionResult<PagingData>> GetSuppliersByPage([FromQuery] int page, [FromQuery] int record)
        {
            var pagingData = new PagingData();
            //Tổng số bản ghi
            var records = await _db.Products.OrderByDescending(x => x.CreateDate).ToListAsync();
            pagingData.TotalRecord = records.Count();
            //Tổng số trang
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page - 1) * record).Take(record).ToList();
            return pagingData;
        }
        /// <summary>
        /// Lấy thông tin chi tiết product theo id
        /// </summary>
        /// <param name="id">id của product</param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/detail?id=7e8dbefb-74e6-46c5-9386-302008af7fb3
        [HttpGet("detail")]
        public async Task<ServiceResponse> GetProduct(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var userInfo = _cache.Get("account_info");
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                res.Message = "Không tìm thấy sản phẩm này";
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("product", product);
            //Lấy product review
            var product_review = _db.ProductReviews.Where(_ => _.ProductId == product.ProductId);
            result.Add("product_review", product_review);
            //Lấy product meta
            var product_meta = _db.ProductMeta.Where(_ => _.ProductId == product.ProductId);
            result.Add("product_meta", product_meta);
            //Lấy category của product
            string sql_get_category = $"select * from category where category_id in (select category_id from product_category where product_id = '{product.ProductId}')";
            var categories = _db.Categories.FromSqlRaw(sql_get_category).ToList();
            result.Add("categories", categories);
            ////Lấy category của product
            string sql_get_tag = $"select * from tag where tag_id in (select tag_id from product_tag where product_id = '{product.ProductId}')";
            var tags = _db.Tags.FromSqlRaw(sql_get_tag).ToList();
            result.Add("tags", tags);
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
        public async Task<ServiceResponse> PostProduct(Product product)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                product.ProductId = Guid.NewGuid();
                _db.Products.Add(product);
                if(product.ProductCategories.Count > 0)
                {
                    foreach (var item in product.ProductCategories)
                    {
                        item.ProductId = product.ProductId;
                    }
                    _db.ProductCategories.AddRange(product.ProductCategories);
                }
                if (product.ProductMetas.Count > 0)
                {
                    foreach (var item in product.ProductMetas)
                    {
                        item.ProductId = product.ProductId;
                    }
                    _db.ProductMeta.AddRange(product.ProductMetas);
                }
                if (product.ProductTags.Count > 0)
                {
                    foreach(var item in product.ProductTags)
                    {
                        item.ProductId = product.ProductId;
                    }
                    _db.ProductTags.AddRange(product.ProductTags);
                }
                await _db.SaveChangesAsync();
                res.Success = true;
                res.Data = _db.Products.Where(_ => _.ProductCode == product.ProductCode).FirstOrDefault();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.ErrorCode = 500;
            }
            return res;
        }
        /// <summary>
        /// Thêm review
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/review
        [HttpPost("review")]
        public async Task<ServiceResponse> AddProductReview(ProductReview review)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _db.ProductReviews.Add(review);
                await _db.SaveChangesAsync();
                res.Success = true;
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Success = false;
                res.ErrorCode = 500;
            }
            return res;
        }
        /// <summary>
        /// Sửa thông tin product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/edit
        [HttpPut("edit")]
        public async Task<ServiceResponse> PutAccount(Product product)
        {
            ServiceResponse res = new ServiceResponse();
            
            try
            {
                _db.Entry(product).State = EntityState.Modified;
                //Xử lý các bảng liên quan
                if (product.ProductCategories.Count > 0)
                {
                    var lstDelete = new List<ProductCategory>();
                    var lstInsert = new List<ProductCategory>();
                    foreach (var item in product.ProductCategories)
                    {
                        switch (item.State)
                        {
                            case 0:
                                lstDelete.Add(item);
                                break;
                            case 2:
                                lstInsert.Add(item);
                                break;
                            default:
                                break;
                        }
                    }
                    if (lstDelete.Count > 0)
                    {
                        _db.ProductCategories.RemoveRange(lstDelete);
                    }
                    if (lstInsert.Count > 0)
                    {
                        _db.ProductCategories.AddRange(lstInsert);
                    }
                }
                if (product.ProductTags.Count > 0)
                {
                    var lstDelete = new List<ProductTag>();
                    var lstInsert = new List<ProductTag>();
                    foreach (var item in product.ProductTags)
                    {
                        switch (item.State)
                        {
                            case 0:
                                lstDelete.Add(item);
                                break;
                            case 2:
                                lstInsert.Add(item);
                                break;
                            default:
                                break;
                        }
                    }
                    if (lstDelete.Count > 0)
                    {
                        _db.ProductTags.RemoveRange(lstDelete);
                    }
                    if (lstInsert.Count > 0)
                    {
                        _db.ProductTags.AddRange(lstInsert);
                    }
                }
                if (product.ProductMetas.Count > 0)
                {
                    var lstDelete = new List<ProductMetum>();
                    var lstInsert = new List<ProductMetum>();
                    foreach (var item in product.ProductMetas)
                    {
                        switch (item.State)
                        {
                            case 0:
                                lstDelete.Add(item);
                                break;
                            case 2:
                                lstInsert.Add(item);
                                break;
                            default:
                                break;
                        }
                    }
                    if (lstDelete.Count > 0)
                    {
                        _db.ProductMeta.RemoveRange(lstDelete);
                    }
                    if (lstInsert.Count > 0)
                    {
                        _db.ProductMeta.AddRange(lstInsert);
                    }
                }
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
        /// Xóa 1 product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    res.Message = "Sản phẩm này không tồn tại";
                    res.ErrorCode = 404;
                    res.Success = false;
                }

                _db.Products.Remove(product);
                //delete ở các bảng liên quan 
                var lstProductCategories = _db.ProductCategories.Where(_ => _.ProductId == product.ProductId);
                _db.ProductCategories.RemoveRange(lstProductCategories);
                var lstProductMetas = _db.ProductMeta.Where(_ => _.ProductId == product.ProductId);
                _db.ProductMeta.RemoveRange(lstProductMetas);
                var lstProductTags = _db.ProductTags.Where(_ => _.ProductId == product.ProductId);
                _db.ProductTags.RemoveRange(lstProductTags);
                await _db.SaveChangesAsync();
                res.Data = product;
            }
            catch
            {
                res.Success = false;
                res.ErrorCode = 500;
                res.Message = "Có lỗi sảy ra";
            }
            return res;
        }
    }
}