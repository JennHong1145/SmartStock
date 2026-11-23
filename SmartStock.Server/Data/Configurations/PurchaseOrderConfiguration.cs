using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
	{
		public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
		{
			// Store the enum as text ("Draft", "Ordered"...) instead of 0, 1, 2...
			builder.Property(po => po.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(po => po.TotalAmount)
				.HasPrecision(18, 2);

			// Supplier with orders can't be deleted
			builder.HasOne(po => po.Supplier)
				.WithMany()
				.HasForeignKey(po => po.SupplierId)
				.OnDelete(DeleteBehavior.Restrict);

			// Deleting an order deletes its lines
			builder.HasMany(po => po.Items)
				.WithOne(i => i.PurchaseOrder)
				.HasForeignKey(i => i.PurchaseOrderId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}