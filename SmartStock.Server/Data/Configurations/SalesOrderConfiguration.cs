using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
	{
		public void Configure(EntityTypeBuilder<SalesOrder> builder)
		{
			builder.Property(so => so.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(so => so.TotalAmount)
				.HasPrecision(18, 2);

			builder.Property(so => so.RowVersion)
				.IsRowVersion();

			// A customer with orders can't be deleted
			builder.HasOne(so => so.Customer)
				.WithMany()
				.HasForeignKey(so => so.CustomerId)
				.OnDelete(DeleteBehavior.Restrict);

			// Deleting an order deletes its lines
			builder.HasMany(so => so.Items)
				.WithOne(i => i.SalesOrder)
				.HasForeignKey(i => i.SalesOrderId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}