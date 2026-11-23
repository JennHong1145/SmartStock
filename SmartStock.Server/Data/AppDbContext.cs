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
		public DbSet<StockMovement> StockMovements { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		}

		public override int SaveChanges()
		{
			ApplyAuditAndSoftDelete();
			return base.SaveChanges();
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ApplyAuditAndSoftDelete();
			return base.SaveChangesAsync(cancellationToken);
		}

		private void ApplyAuditAndSoftDelete()
		{
			var now = DateTime.UtcNow;

			// 1. Soft delete: turn a delete into an update (IsDeleted = true)
			var deletedEntries = ChangeTracker.Entries<ISoftDelete>()
				.Where(e => e.State == EntityState.Deleted)
				.ToList();

			foreach (var entry in deletedEntries)
			{
				entry.State = EntityState.Modified;
				entry.Entity.IsDeleted = true;
			}

			// 2. Auditing: fill the dates automatically
			foreach (var entry in ChangeTracker.Entries<IAuditable>())
			{
				if (entry.State == EntityState.Added)
				{
					entry.Entity.CreatedAt = now;
				}
				else if (entry.State == EntityState.Modified)
				{
					entry.Entity.UpdatedAt = now;
					entry.Property(e => e.CreatedAt).IsModified = false;
				}
			}
		}
	}
}