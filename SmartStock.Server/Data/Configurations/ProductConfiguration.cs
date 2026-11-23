using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class ProductConfiguration : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.Property(p => p.SKU)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(p => p.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(p => p.Price)
				.HasPrecision(18, 2);

			builder.Property(p => p.RowVersion)
				.IsRowVersion();

			builder.Property(p => p.CreatedBy).HasMaxLength(450);
			builder.Property(p => p.UpdatedBy).HasMaxLength(450);

			// Soft delete: deleted products disappear from normal queries
			builder.HasQueryFilter(p => !p.IsDeleted);

			builder.HasIndex(p => p.SKU)
				.IsUnique();

			builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "[Price] >= 0"));

			// Fixed date: HasData must be deterministic, so never use DateTime.UtcNow here
			var seedDate = new DateTime(2025, 1, 1);

			builder.HasData(
				new Product { Id = 1, SKU = "ELEC-001", Name = "Laptop 14\"", Price = 1299.00m, CategoryId = 1, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 2, SKU = "ELEC-002", Name = "Smartphone", Price = 799.50m, CategoryId = 1, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 3, SKU = "ACC-001", Name = "USB-C Cable", Price = 9.99m, CategoryId = 2, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 4, SKU = "ACC-002", Name = "Wireless Mouse", Price = 24.90m, CategoryId = 2, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 5, SKU = "OFF-001", Name = "A4 Paper Ream", Price = 5.50m, CategoryId = 3, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 6, SKU = "OFF-002", Name = "Ballpoint Pens (10)", Price = 3.20m, CategoryId = 3, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 7, SKU = "BEV-001", Name = "Bottled Water 500ml", Price = 1.00m, CategoryId = 4, IsActive = true, CreatedAt = seedDate },
				new Product { Id = 8, SKU = "BEV-002", Name = "Orange Juice 1L", Price = 3.75m, CategoryId = 4, IsActive = false, CreatedAt = seedDate }
			);
		}
	}
}