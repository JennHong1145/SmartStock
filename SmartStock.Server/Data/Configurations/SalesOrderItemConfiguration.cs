using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
	{
		public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
		{
			builder.Property(i => i.UnitPrice)
				.HasPrecision(18, 2);

			// A product with sales history can't be deleted
			builder.HasOne(i => i.Product)
				.WithMany()
				.HasForeignKey(i => i.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}