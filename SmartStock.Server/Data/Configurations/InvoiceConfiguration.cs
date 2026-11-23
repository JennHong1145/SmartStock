using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
	{
		public void Configure(EntityTypeBuilder<Invoice> builder)
		{
			builder.Property(i => i.InvoiceNumber)
				.IsRequired()
				.HasMaxLength(50);

			builder.HasIndex(i => i.InvoiceNumber)
				.IsUnique();

			// One-to-one: Invoice owns the foreign key
			builder.HasOne(i => i.SalesOrder)
				.WithOne(o => o.Invoice)
				.HasForeignKey<Invoice>(i => i.SalesOrderId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}