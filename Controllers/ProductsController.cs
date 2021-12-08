
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sales_Model.Common;
using Sales_Model.Constants;
using Sales_Model.Model;
using Sales_Model.Model.ModelCustom.Product;
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
        /// https://localhost:44335/api/products?page=2&record=10&search=mô+hình
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PagingData>> GetSuppliersByPage(
            [FromQuery] string search,
            [FromQuery] string sort,
            [FromQuery] int? page = 1,
            [FromQuery] int? record = 20)
        {
            var pagingData = new PagingData();
            List<Product> records = await _db.Products.ToListAsync();
            //Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                string sql_get_product = "select * from product where CHARINDEX(@txtSeach, product_code) > 0 or CHARINDEX(@txtSeach, title) > 0" +
                                                                    "or CHARINDEX(@txtSeach, metaTitle) > 0 or CHARINDEX(@txtSeach, slug) > 0" +
                                                                    "or CHARINDEX(@txtSeach, sku) > 0 or CHARINDEX(@txtSeach, content) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.Products.FromSqlRaw(sql_get_product, param).OrderByDescending(x => x.CreateDate).ToList();
            }
            //Sắp xếp 
            if (!string.IsNullOrEmpty(sort))
            {
                records = Helper.OrderBy<Product>(records, sort).ToList();
            }
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();

            return pagingData;
        }

        /// <summary>
        /// Lấy thông tin chi tiết product theo id
        /// </summary>
        /// <param name="id">id của product</param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/detail?id=7e8dbefb-74e6-46c5-9386-302008af7fb3
        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<ServiceResponse> GetProduct(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var userInfo = _cache.Get("account_info");
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                res.Message = Message.ProductNotFound;
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
            var param = new SqlParameter("@product_id", product.ProductId);
            //Lấy category của product
            string sql_get_category = $"select * from category where category_id in (select category_id from product_category where product_id = @product_id)";
            var categories = _db.Categories.FromSqlRaw(sql_get_category, param).ToList();
            result.Add("categories", categories);
            ////Lấy category của product
            string sql_get_tag = $"select * from tag where tag_id in (select tag_id from product_tag where product_id = @product_id)";
            var tags = _db.Tags.FromSqlRaw(sql_get_tag, param).ToList();
            result.Add("tags", tags);
            res.Data = result;
            res.Success = true;
            return res;
        }

        /// <summary>
        /// Lấy thông tin chi tiết product theo slug url
        /// </summary>
        /// <param name="slug">slug url của product</param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/detail/slug?id=7e8dbefb-74e6-46c5-9386-302008af7fb3
        [AllowAnonymous]
        [HttpGet("detail/slug")]
        public async Task<ServiceResponse> GetProductBySlug(string slug)
        {
            ServiceResponse res = new ServiceResponse();
            var product = await _db.Products.Where(p => p.Slug.Equals(slug.Trim())).FirstOrDefaultAsync();
            if (product == null)
            {
                res.Message = Message.ProductNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            try
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("product", product);
                //Lấy product review
                var product_review = _db.ProductReviews.Where(_ => _.ProductId == product.ProductId);
                result.Add("product_review", product_review);
                //Lấy product meta
                var product_meta = _db.ProductMeta.Where(_ => _.ProductId == product.ProductId);
                result.Add("product_meta", product_meta);
                var param = new SqlParameter("@product_id", product.ProductId);
                //Lấy category của product
                string sql_get_category = $"select * from category where category_id in (select category_id from product_category where product_id = @product_id)";
                var categories = _db.Categories.FromSqlRaw(sql_get_category, param).ToList();
                result.Add("categories", categories);
                //Lấy tag của product
                string sql_get_tag = $"select * from tag where tag_id in (select tag_id from product_tag where product_id = @product_id)";
                var tags = _db.Tags.FromSqlRaw(sql_get_tag, param).ToList();
                result.Add("tags", tags);
                res.Data = result;
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

        /// <summary>
        /// Thêm product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products
        [HttpPost]
        public async Task<ServiceResponse> PostProduct(ProductRequest product)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "AddProduct"))//Check quyền
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
                return res;
            }
            try
            {
                product.ProductId = Guid.NewGuid();
                product.Title = product.Title.Trim();
                product.ProductImage = product.ProductImage.Trim();
                product.Slug = SlugGenerator.SlugGenerator.GenerateSlug(product.Title.Trim()) + "-" + DateTime.Now.ToFileTime().ToString();
                if (product.ProductCategories != null && product.ProductCategories.Count > 0)
                {
                    var list = new List<ProductCategory>();
                    foreach (var categoryId in product.ProductCategories)
                    {
                        var category = new ProductCategory
                        {
                            ProductId = product.ProductId,
                            CategoryId = categoryId,
                        };
                        list.Add(category);
                    }
                    _db.ProductCategories.AddRange(list);
                }
                //if (product.ProductMetas != null && product.ProductMetas.Count > 0)
                //{
                //    foreach (var item in product.ProductMetas)
                //    {
                //        item.ProductId = product.ProductId;
                //    }
                //    _db.ProductMeta.AddRange(product.ProductMetas);
                //}
                //if (product.ProductTags != null && product.ProductTags.Count > 0)
                //{
                //    foreach (var item in product.ProductTags)
                //    {
                //        item.ProductId = product.ProductId;
                //    }
                //    _db.ProductTags.AddRange(product.ProductTags);
                //}

                var productAddInstance = new Product
                {
                    ProductId = product.ProductId,
                    AccountId = null,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    ProductPrimaryImage = product.ProductPrimaryImage,
                    ImportPrice = product.ImportPrice,
                    SellingPrice = product.SellingPrice,
                    Title = product.Title,
                    MetaTitle = product.MetaTitle,
                    Slug = product.Slug,
                    Summary = product.Summary,
                    Type = product.Type,
                    Sku = product.Sku,
                    Price = product.Price,
                    Discount = product.Discount,
                    Quantity = product.Quantity,
                    Shop = product.Shop,
                    Content = product.Content,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    PublishedDate = DateTime.Now,
                    StartDate = DateTime.Now,
                    EndDate = null,
                };

                await _db.Products.AddAsync(productAddInstance);
                await _db.SaveChangesAsync();
                Helper.WriteLogAsync(HttpContext, Message.ProductLogAdd);
                res.Success = true;
                res.Data = _db.Products.Where(_ => _.ProductCode == product.ProductCode).FirstOrDefault();
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
                res.Message = Message.ErrorMsg;
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
        [HttpPost("edit")]
        public async Task<ServiceResponse> PutAccount(Product product)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "EditProduct"))//Check quyền
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
                return res;
            }
            Product productDb = await _db.Products.FindAsync(product.ProductId);
            if (productDb == null)
            {
                res.Message = Message.ProductNotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            try
            {
                //Chỉ cập nhật những thông tin được phép thay đổi
                productDb.ProductCode = product.ProductCode != null ? product.ProductCode : productDb.ProductCode;
                productDb.Title = product.Title?.Trim();
                productDb.MetaTitle = product.MetaTitle != null ? product.MetaTitle.Trim() : productDb.MetaTitle;
                productDb.Slug = SlugGenerator.SlugGenerator.GenerateSlug(product.Title?.Trim()) + "-" + DateTime.Now.ToFileTime().ToString(); ;
                productDb.Summary = product.Summary != null ? product.Summary.Trim() : productDb.Summary;
                productDb.Type = product.Type != null ? product.Type : productDb.Type;
                productDb.Sku = product.Sku != null ? product.Sku.Trim() : productDb.Sku;
                productDb.Price = product.Price != null ? product.Price : productDb.Price;
                productDb.ImportPrice = product.ImportPrice != null ? product.ImportPrice : productDb.ImportPrice;
                productDb.Discount = product.Discount != null ? product.Discount : productDb.Discount;
                productDb.Quantity = product.Quantity != null ? product.Quantity : productDb.Quantity;
                productDb.Shop = product.Shop != null ? product.Shop : productDb.Shop;
                productDb.Content = product.Content != null ? product.Content.Trim() : productDb.Content;
                productDb.ProductImage = product.ProductImage != null ? product.ProductImage.Trim() : productDb.ProductImage;
                //Xử lý các bảng liên quan
                if (product.ProductCategories != null && product.ProductCategories.Count > 0)
                {
                    var lstDelete = new List<ProductCategory>();
                    var lstInsert = new List<ProductCategory>();
                    foreach (var item in product.ProductCategories)
                    {
                        if (item.State == null) item.State = 0;
                        switch (item.State)
                        {
                            case (int)RecordStatus.Delete:
                                lstDelete.Add(item);
                                break;
                            case (int)RecordStatus.Add:
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
                if (product.ProductTags != null && product.ProductTags.Count > 0)
                {
                    var lstDelete = new List<ProductTag>();
                    var lstInsert = new List<ProductTag>();
                    foreach (var item in product.ProductTags)
                    {
                        if (item.State == null) item.State = 0;
                        switch (item.State)
                        {
                            case (int)RecordStatus.Delete:
                                lstDelete.Add(item);
                                break;
                            case (int)RecordStatus.Add:
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
                if (product.ProductMetas != null && product.ProductMetas.Count > 0)
                {
                    var lstDelete = new List<ProductMetum>();
                    var lstInsert = new List<ProductMetum>();
                    foreach (var item in product.ProductMetas)
                    {
                        if (item.State == null) item.State = 0;
                        switch (item.State)
                        {
                            case (int)RecordStatus.Delete:
                                lstDelete.Add(item);
                                break;
                            case (int)RecordStatus.Add:
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
                Helper.WriteLogAsync(HttpContext, Message.ProductLogChange);
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
        /// Xóa 1 product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/7e8dbefb-74e6-46c5-9386-302008af7fb3
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteAccount(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            if (!Helper.CheckPermission(HttpContext, "DeleteProduct"))//Check quyền xóa
            {
                res.Success = false;
                res.Message = Message.NotAuthorize;
                res.ErrorCode = 403;
                res.Data = Message.NotAuthorize;
                return res;
            }
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    res.Message = Message.ProductNotExist;
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
                Helper.WriteLogAsync(HttpContext, Message.ProductLogDelete);
                res.Data = product;
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
        /// Thêm category
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/category
        [HttpPost("category")]
        public async Task<ServiceResponse> AddProductCategory(ProductCategory category)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _db.ProductCategories.Add(category);
                await _db.SaveChangesAsync();
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

        /// <summary>
        /// Xoá category
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        /// https://localhost:44335/api/products/category
        [HttpDelete("category")]
        public async Task<ServiceResponse> DeleteProductCategory(ProductCategory category)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _db.ProductCategories.Remove(category);
                await _db.SaveChangesAsync();
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

        /// <summary>
        /// Lấy danh sách product theo category
        /// </summary>
        /// <param name="page">trang</param>
        /// <param name="record">số bản ghi trên 1 trang</param>
        /// <returns></returns>
        /// https://localhost:44335/api/products?page=2&record=10&search=mô+hình
        [AllowAnonymous]
        [HttpPost("by_category")]
        public async Task<ActionResult<ServiceResponse>> GetProductByCategory(List<Guid?> categoryIds)
        {
            ServiceResponse res = new ServiceResponse();
            //Tổng số bản ghi
            //var data = _db.Products.Join(_db.Categories, product => product.ProductId, category => category.CategoryId,
            //    (product, category) => new
            //    {
            //        CategoryId = category.CategoryId,
            //        product = product,

            //    }).ToListAsync();
            //foreach(var id in categoryIds)
            //{
            //    var category = await _db.Categories.FindAsync(id);
            //    if (category == null)
            //    {
            //        res.Message = Message.CategoryNotFound;
            //        res.ErrorCode = 404;
            //        res.Success = false;
            //        res.Data = null;
            //    }
            //    string sql_get_category = $"select * from product p inner join product_category pc on p.id = pc.product_id where pc.category_id <> pc.category_id in (@category_id)";
            //    var param = new SqlParameter("@category_id", category.CategoryId);
            //    var product = _db.Products.FromSqlRaw(sql_get_category, param).ToList();
            //    data.AddRange(product);
            //}
            var test = await _db.Categories.ToListAsync();
            string sql_get_category = $"select * " +
                                        $"from product p where p.product_id in (select pc.product_id from product_category pc where pc.category_id in (@category_ids))";
            var data = _db.Products.FromSqlRaw(sql_get_category, new SqlParameter("@category_ids", String.Join(", ", categoryIds.ToArray()))).ToList();
            res.Data = data;
            res.Success = true;
            return res;
        }
    }
}
