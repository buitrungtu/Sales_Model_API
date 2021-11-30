using Microsoft.AspNetCore.Authorization;
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
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sales_Model.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Sales_ModelContext _db;
        private IMemoryCache _cache;

        public CategoryController(Sales_ModelContext context, IMemoryCache memoryCache)
        {
            _db = context;
            _cache = memoryCache;

        }

        /// <summary>
        /// Thêm category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceResponse> AddCategory(Category category)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                if (category.Title == null || string.IsNullOrEmpty(category.Title))
                {
                    res.Message = Message.TitleError;
                    res.Success = false;
                    res.ErrorCode = 400;

                    return res;
                }
                category.CategoryId = Guid.NewGuid();
                category.Title = category.Title.Trim();
                category.CategoryCode = category.CategoryCode.Trim();
                category.Slug = SlugGenerator.SlugGenerator.GenerateSlug(category.Title.Trim()) + "-" + DateTime.Now.ToFileTime().ToString();
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();

                res.Success = true;
                res.Data = category;
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Message = Message.ErrorMsg;
                res.Success = false;
                res.ErrorCode = 500;
            }
            return res;
        }

        /// <summary>
        /// Lấy danh sách category có phân trang và cho phép tìm kiếm
        /// </summary>
        /// <returns></returns>
        /// https://localhost:44335/api/category?page=2&record=10&search=moHinh
        [AllowAnonymous]
        [HttpGet]
        public async Task<PagingData> GetCategoriesByPagingAndSearch([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 10)
        {
            var pagingData = new PagingData();
            List<Category> records = new List<Category>();
            //Tổng số bản ghi
            if (search != null && search.Trim() != "")
            {
                //CHARINDEX tìm không phân biệt hoa thường trả về vị trí đầu tiên xuất hiện của chuỗi con
                string sql_get_category = "select * from category where CHARINDEX(@txtSeach, title) > 0 or CHARINDEX(@txtSeach, slug) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.Categories.FromSqlRaw(sql_get_category, param).OrderByDescending(x => x.Title).ToList();
            }
            else
            {
                records = await _db.Categories.OrderByDescending(x => x.Title).ToListAsync();
            }
            pagingData.TotalRecord = records.Count(); //Tổng số bản ghi
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value)); //Tổng số trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList(); //Dữ liệu của từng trang

            return pagingData;
        }
        
        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<ServiceResponse> GetCategoryDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                res.Message = Message.ProductNotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("category", category);

            res.Data = result;
            res.Success = true;
            return res;
        }
        
        /// <summary>
        /// Xoá category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ServiceResponse> DeleteCategory(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var category = await _db.Categories.FindAsync(id);
                if (category == null)
                {
                    res.Message = Message.CategoryNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    return res;
                }
                _db.Categories.Remove(category);
                res.Success = true;
                res.Data = null;
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                res.Message = Message.ErrorMsg;
                res.Success = false;
                res.ErrorCode = 500;
            }
            return res;
        }
    }
}
