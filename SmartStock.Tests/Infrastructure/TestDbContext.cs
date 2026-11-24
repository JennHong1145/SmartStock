using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartStock.Server.Data;

namespace SmartStock.Tests.Infrastructure
{
	// Same model as AppDbContext, with the SQL Server-only RowVersion generation switched off
	public class TestDbContext : AppDbContext
	{
		public TestDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var rowVersion = entityType.FindProperty("RowVersion");
				if (rowVersion != null && rowVersion.ClrType == typeof(byte[]))
				{
					rowVersion.IsConcurrencyToken = false;
					rowVersion.ValueGenerated = ValueGenerated.Never;
				}
			}
		}
	}
}