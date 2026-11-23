using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
	{
		public void Configure(EntityTypeBuilder<Customer> builder)
		{
			builder.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(c => c.Email)
				.HasMaxLength(256);

			builder.Property(c => c.Phone)
				.HasMaxLength(30);

			builder.Property(c => c.Address)
				.HasMaxLength(250);

			builder.Property(c => c.CreatedBy).HasMaxLength(450);
			builder.Property(c => c.UpdatedBy).HasMaxLength(450);

			// Customers are searched by email
			builder.HasIndex(c => c.Email);

			builder.HasQueryFilter(c => !c.IsDeleted);
		}
	}
}