using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        [AllowAnonymous]
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
                category.Slug = SlugGenerator.SlugGenerator.GenerateSlug(category.Title.Trim()) + DateTime.Now.ToFileTime().ToString();
                _db.Categories.Add(category);
                res.Success = true;
                res.Data = category;
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

        /// <summary>
        /// Xoá category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpDelete]
        public async Task<ServiceResponse> DeleteCategory(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var category = _db.Categories.FindAsync(id);
                if (category == null)
                {
                    res.Message = Message.CategoryNotFound;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                }
                res.Success = true;
                res.Data = category;
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
