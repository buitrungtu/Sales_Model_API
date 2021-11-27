using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Product
    {
        public Guid ProductId { get; set; }
        public Guid? AccountId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductPrimaryImage { get; set; }
        public float ImportPrice { get; set; }
        public float SellingPrice { get; set; }

        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
        public int? Type { get; set; }
        public string Sku { get; set; }
        public double? Price { get; set; }
        public double? Discount { get; set; }
        public int? Quantity { get; set; }
        public int? Shop { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductImage { get; set; }

        [NotMapped]
        public List<ProductCategory> ProductCategories { get; set; }
        [NotMapped]
        public List<ProductTag> ProductTags { get; set; }
        [NotMapped]
        public List<ProductMetum> ProductMetas { get; set; }
    }
}
