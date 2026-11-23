using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data
{
	// Development-only sample data. The small fixed lookups stay in HasData.
	public static class DataSeeder
	{
		public static async Task SeedAsync(AppDbContext db)
		{
			// Skip only if this seeder has already run (its sample SKUs exist)
			if (await db.Products.IgnoreQueryFilters().AnyAsync(p => p.SKU.StartsWith("SMP-")))
				return;

			// Customers
			var customers = Enumerable.Range(1, 20).Select(i => new Customer
			{
				Name = $"Sample Customer {i:D2}",
				Email = $"customer{i:D2}@example.com",
				Phone = $"555-01{i:D2}",
				Address = $"{i} Sample Street"
			}).ToList();
			db.Customers.AddRange(customers);

			// Suppliers
			var suppliers = Enumerable.Range(1, 5).Select(i => new Supplier
			{
				Name = $"Sample Supplier {i}",
				ContactEmail = $"supplier{i}@example.com",
				Phone = $"555-02{i:D2}",
				Address = $"{i} Warehouse Road"
			}).ToList();
			db.Suppliers.AddRange(suppliers);

			// Products spread over the 4 seeded categories (ids 1-4)
			var random = new Random(42); // fixed seed so the data is repeatable
			var products = Enumerable.Range(1, 30).Select(i => new Product
			{
				SKU = $"SMP-{i:D3}",
				Name = $"Sample Product {i:D2}",
				Description = "Sample data for development",
				Price = Math.Round((decimal)(random.NextDouble() * 200 + 1), 2),
				CategoryId = (i % 4) + 1,
				IsActive = true
			}).ToList();
			db.Products.AddRange(products);

			await db.SaveChangesAsync(); // goes through the audit override, so CreatedAt is filled

			// Opening stock goes through the ledger, never a stock column
			var movements = products.Select(p => new StockMovement
			{
				ProductId = p.Id,
				MovementType = StockMovementType.PurchaseIn,
				Quantity = random.Next(20, 200),
				MovementDate = DateTime.UtcNow,
				Notes = "Development opening stock"
			});
			db.StockMovements.AddRange(movements);

			await db.SaveChangesAsync();
		}
	}
}