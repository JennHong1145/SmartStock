using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class CategoryConfiguration : IEntityTypeConfiguration<Category>
	{
		public void Configure(EntityTypeBuilder<Category> builder)
		{
			builder.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(100);

			// Self-reference: Restrict, because SQL Server rejects multiple cascade paths
			builder.HasOne(c => c.ParentCategory)
				.WithMany(c => c.SubCategories)
				.HasForeignKey(c => c.ParentCategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			// One category has many products. Restrict: a category with products can't be deleted
			builder.HasMany(c => c.Products)
				.WithOne(p => p.Category)
				.HasForeignKey(p => p.CategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasData(
				new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
				new Category { Id = 2, Name = "Accessories", Description = "Electronic accessories", ParentCategoryId = 1 },
				new Category { Id = 3, Name = "Office Supplies", Description = "Stationery and office items" },
				new Category { Id = 4, Name = "Beverages", Description = "Drinks" }
			);
		}
	}
}