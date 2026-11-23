using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<Category> Categories { get; set; }

		public DbSet<Product> Products { get; set; }

		public DbSet<Supplier> Suppliers { get; set; }

		public DbSet<Customer> Customers { get; set; }

		public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

		public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
		public DbSet<SalesOrder> SalesOrders { get; set; }

		public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

		public DbSet<Invoice> Invoices { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}
	}
}