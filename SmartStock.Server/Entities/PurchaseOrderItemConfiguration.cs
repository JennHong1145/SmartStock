using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
	{
		public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
		{
			builder.Property(i => i.UnitCost)
				.HasPrecision(18, 2);

			// A product with order history can't be deleted
			builder.HasOne(i => i.Product)
				.WithMany()
				.HasForeignKey(i => i.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}