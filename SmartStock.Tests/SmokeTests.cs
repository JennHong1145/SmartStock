using Microsoft.EntityFrameworkCore;
using SmartStock.Tests.Infrastructure;

namespace SmartStock.Tests
{
	public class SmokeTests
	{
		[Fact]
		public async Task Database_is_created_with_the_seeded_products()
		{
			using var test = new TestDatabase();

			var count = await test.Db.Products.CountAsync();

			Assert.Equal(8, count); // the 8 HasData products
		}
	}
}