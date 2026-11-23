using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data.Configurations
{
	public class SalesSummaryByProductConfiguration : IEntityTypeConfiguration<SalesSummaryByProduct>
	{
		public void Configure(EntityTypeBuilder<SalesSummaryByProduct> builder)
		{
			// HasNoKey: no primary key. ToView: read from the view, so migrations won't create a table.
			builder.HasNoKey().ToView("vw_SalesSummaryByProduct");

			builder.Property(s => s.TotalRevenue)
				.HasPrecision(18, 2);
		}
	}
}