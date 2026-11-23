using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
	{
		public void Configure(EntityTypeBuilder<StockMovement> builder)
		{
			// Store the enum as text ("PurchaseIn", "SaleOut"...)
			builder.Property(m => m.MovementType)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(m => m.Notes)
				.HasMaxLength(250);

			// A product with stock history can't be deleted
			builder.HasOne(m => m.Product)
				.WithMany()
				.HasForeignKey(m => m.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			// Composite index for "current stock" and "stock history" queries
			builder.HasIndex(m => new { m.ProductId, m.MovementDate });
		}
	}
}