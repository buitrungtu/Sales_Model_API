using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sales_Model.Model;

#nullable disable

namespace Sales_Model.OutputDirectory
{
    public partial class Sales_ModelContext : DbContext
    {
        public Sales_ModelContext()
        {
        }

        public Sales_ModelContext(DbContextOptions<Sales_ModelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountInfo> AccountInfos { get; set; }
        public virtual DbSet<AccountRole> AccountRoles { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrdersItem> OrdersItems { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ProductMetum> ProductMeta { get; set; }
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<ProductTag> ProductTags { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Auditinglog> Auditinglogs { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("workstation id=SalesModel.mssql.somee.com;packet size=4096;user id=tudefttry_SQLLogin_1;pwd=utnl7fvzym;data source=SalesModel.mssql.somee.com;persist security info=False;initial catalog=SalesModel");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("address");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(255)
                    .HasColumnName("display_name");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.EmailBackup)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email_backup");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.IsInterestedAccount).HasColumnName("is_interested_account");

                entity.Property(e => e.LastAction)
                    .HasColumnType("datetime")
                    .HasColumnName("last_action")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastIp)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("last_ip");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("last_login")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("mobile");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<AccountInfo>(entity =>
            {
                entity.ToTable("account_info");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasColumnName("country");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("firstName");

                entity.Property(e => e.Intro)
                    .HasMaxLength(255)
                    .HasColumnName("intro");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(255)
                    .HasColumnName("display_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("lastName");

                entity.Property(e => e.Line1)
                    .HasMaxLength(50)
                    .HasColumnName("line1");

                entity.Property(e => e.Line2)
                    .HasMaxLength(50)
                    .HasColumnName("line2");

                entity.Property(e => e.MidName)
                    .HasMaxLength(50)
                    .HasColumnName("midName");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("mobile");

                entity.Property(e => e.Profile).HasColumnName("profile");

                entity.Property(e => e.Province)
                    .HasMaxLength(50)
                    .HasColumnName("province");
            });

            modelBuilder.Entity<AccountRole>(entity =>
            {
                //entity.HasNoKey();
                entity.HasKey(x => new { x.AccountId, x.RoleId });

                entity.ToTable("account_role");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("cart");

                entity.Property(e => e.CartId)
                    .HasColumnName("cart_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SessionId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("session_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Token)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("token");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_item");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.CartId).HasColumnName("cart_id");

                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("code");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Sku)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("sku");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");

                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CategoryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("category_code");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.MetaTitle)
                    .HasMaxLength(255)
                    .HasColumnName("metaTitle");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .HasColumnName("slug");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrdersId)
                    .HasName("PK__orders__B46F6833653CCBCC");

                entity.ToTable("orders");

                entity.Property(e => e.OrdersId)
                    .HasColumnName("orders_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.GrandTotal).HasColumnName("grandTotal");

                entity.Property(e => e.ItemDiscount).HasColumnName("itemDiscount");

                entity.Property(e => e.Promo)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("promo");

                entity.Property(e => e.SessionId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("session_id");

                entity.Property(e => e.Shipping).HasColumnName("shipping");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.SubTotal).HasColumnName("subTotal");

                entity.Property(e => e.Tax).HasColumnName("tax");

                entity.Property(e => e.Token)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("token");

                entity.Property(e => e.Total).HasColumnName("total");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CustomerName).HasColumnName("CustomerName");
                entity.Property(e => e.CustomerPhone).HasColumnName("CustomerPhone");
                entity.Property(e => e.CustomerAddress).HasColumnName("CustomerAddress");
            });

            modelBuilder.Entity<OrdersItem>(entity =>
            {
                entity.ToTable("orders_item");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Sku)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("sku");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("end_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MetaTitle)
                    .HasMaxLength(255)
                    .HasColumnName("metaTitle");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.ProductCode)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("product_code");

                entity.Property(e => e.PublishedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("published_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Shop).HasColumnName("shop");

                entity.Property(e => e.Sku)
                    .HasMaxLength(100)
                    .HasColumnName("sku");

                entity.Property(e => e.ProductName)
                    .HasMaxLength(255)
                    .HasColumnName("product_name");

                entity.Property(e => e.ProductPrimaryImage)
                    .HasMaxLength(255)
                    .HasColumnName("product_primary_image");

                entity.Property(e => e.ImportPrice)
                    .HasColumnName("import_price");

                entity.Property(e => e.SellingPrice)
                    .HasColumnName("selling_price");

                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .HasColumnName("slug");

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("start_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Summary)
                    .HasMaxLength(100)
                    .HasColumnName("summary");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProductImage)
                    .HasMaxLength(500)
                    .HasColumnName("ProductImage");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                //entity.HasNoKey();
                entity.HasKey(x => new { x.CategoryId, x.ProductId });

                entity.ToTable("product_category");

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");
            });

            modelBuilder.Entity<ProductMetum>(entity =>
            {
                entity.ToTable("product_meta");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.KeyMeta)
                    .HasMaxLength(50)
                    .HasColumnName("key_meta");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .HasColumnName("url");
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("product_review");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.PublishedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("published_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("product_tag");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.TagId).HasColumnName("tag_id");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("role");

                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.Role_Code)
                    .HasMaxLength(50)
                    .HasColumnName("role_code");
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tag");

                entity.Property(e => e.TagId)
                    .HasColumnName("tag_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.MetaTitle)
                    .HasMaxLength(255)
                    .HasColumnName("metaTitle");

                entity.Property(e => e.Slug)
                    .HasMaxLength(100)
                    .HasColumnName("slug");

                entity.Property(e => e.TagCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("tag_code");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");

                entity.Property(e => e.TransactionId)
                    .HasColumnName("transaction_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("account_id");

                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("code");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Mode).HasColumnName("mode");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("update_date")
                    .HasDefaultValueSql("(getdate())");
            });
            modelBuilder.Entity<Auditinglog>(entity =>
            {
                entity.ToTable("auditinglog");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountId).HasColumnName("AccountId");

                entity.Property(e => e.Ip)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("ip");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("Username");

                entity.Property(e => e.Action).HasColumnName("Action");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");
            });
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("address");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("create_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(255)
                    .HasColumnName("display_name");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.EmailBackup)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email_backup");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastIp)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("last_ip");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("last_login")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .HasColumnName("last_name");

                entity.Property(e => e.Mobile)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("mobile");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");
            });
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("product_image");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.AltContent).HasColumnName("alt_content");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .HasColumnName("url");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
