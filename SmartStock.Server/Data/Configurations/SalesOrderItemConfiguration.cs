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

			builder.Property(i => i.LineTotal)
				.HasPrecision(18, 2)
				.HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

			// A product with sales history can't be deleted
			builder.HasOne(i => i.Product)
				.WithMany()
				.HasForeignKey(i => i.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.ToTable(t =>
			{
				t.HasCheckConstraint("CK_SalesOrderItems_Quantity", "[Quantity] >= 0");
				t.HasCheckConstraint("CK_SalesOrderItems_UnitPrice", "[UnitPrice] >= 0");
			});
		}
	}
}