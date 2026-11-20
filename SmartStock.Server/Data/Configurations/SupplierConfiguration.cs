using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
	{
		public void Configure(EntityTypeBuilder<Supplier> builder)
		{
			builder.Property(s => s.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(s => s.ContactEmail)
				.HasMaxLength(256);

			builder.Property(s => s.Phone)
				.HasMaxLength(30);

			builder.Property(s => s.Address)
				.HasMaxLength(250);
		}
	}
}