using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Entities;
using SmartStock.Server.Services;
using SmartStock.Shared.Dtos;
using SmartStock.Tests.Infrastructure;

namespace SmartStock.Tests
{
	public class OrderServiceTests
	{
		// Product 1 (Laptop) comes from HasData. Customers are not seeded, so tests add their own.
		private static async Task<int> AddCustomerAsync(TestDatabase test)
		{
			var customer = new Customer { Name = "Test Customer" };
			test.Db.Customers.Add(customer);
			await test.Db.SaveChangesAsync();
			return customer.Id;
		}

		private static async Task AddStockAsync(TestDatabase test, int productId, int quantity)
		{
			test.Db.StockMovements.Add(new StockMovement
			{
				ProductId = productId,
				MovementType = StockMovementType.PurchaseIn,
				Quantity = quantity,
				MovementDate = DateTime.UtcNow
			});
			await test.Db.SaveChangesAsync();
		}

		private static List<OrderLineRequest> Lines(int productId, int quantity)
			=> new() { new OrderLineRequest { ProductId = productId, Quantity = quantity } };

		[Fact]
		public async Task Valid_order_creates_order_invoice_and_stock_movement()
		{
			using var test = new TestDatabase();
			var customerId = await AddCustomerAsync(test);
			await AddStockAsync(test, productId: 1, quantity: 10);
			var service = test.CreateOrderService();

			var result = await service.PlaceSalesOrderAsync(customerId, Lines(1, 3));

			Assert.Equal(1299.00m * 3, result.TotalAmount);
			Assert.StartsWith("INV-", result.InvoiceNumber);
			Assert.Equal(1, await test.Db.SalesOrders.CountAsync());
			Assert.Equal(1, await test.Db.SalesOrderItems.CountAsync());
			Assert.Equal(1, await test.Db.Invoices.CountAsync());

			var saleOut = await test.Db.StockMovements
				.SingleAsync(m => m.MovementType == StockMovementType.SaleOut);
			Assert.Equal(3, saleOut.Quantity);
			Assert.Equal(result.OrderId, saleOut.ReferenceId);
		}

		[Fact]
		public async Task Order_above_available_stock_is_rejected_and_nothing_is_saved()
		{
			using var test = new TestDatabase();
			var customerId = await AddCustomerAsync(test);
			await AddStockAsync(test, productId: 1, quantity: 5);
			var service = test.CreateOrderService();

			var ex = await Assert.ThrowsAsync<InsufficientStockException>(
				() => service.PlaceSalesOrderAsync(customerId, Lines(1, 6)));

			Assert.Contains("available 5", ex.Message);
			Assert.Equal(0, await test.Db.SalesOrders.CountAsync());
			Assert.Equal(0, await test.Db.Invoices.CountAsync());
			Assert.Equal(0, await test.Db.StockMovements.CountAsync(m => m.MovementType == StockMovementType.SaleOut));
		}

		[Fact]
		public async Task Unknown_customer_is_rejected()
		{
			using var test = new TestDatabase();
			await AddStockAsync(test, productId: 1, quantity: 5);
			var service = test.CreateOrderService();

			var ex = await Assert.ThrowsAsync<BusinessRuleException>(
				() => service.PlaceSalesOrderAsync(99999, Lines(1, 1)));

			Assert.Contains("99999", ex.Message);
			Assert.Equal(0, await test.Db.SalesOrders.CountAsync());
		}

		[Fact]
		public async Task Failure_on_a_later_line_rolls_back_the_whole_order()
		{
			using var test = new TestDatabase();
			var customerId = await AddCustomerAsync(test);
			await AddStockAsync(test, productId: 1, quantity: 10); // line 1 is fine
			// Product 2 has no stock, so line 2 fails after line 1 was already validated
			var service = test.CreateOrderService();
			var lines = new List<OrderLineRequest>
			{
				new() { ProductId = 1, Quantity = 2 },
				new() { ProductId = 2, Quantity = 1 }
			};

			await Assert.ThrowsAsync<InsufficientStockException>(
				() => service.PlaceSalesOrderAsync(customerId, lines));

			Assert.Equal(0, await test.Db.SalesOrders.CountAsync());
			Assert.Equal(0, await test.Db.StockMovements.CountAsync(m => m.MovementType == StockMovementType.SaleOut));
		}

[Fact]
public async Task Invoice_is_created_with_the_right_number_dates_and_order()
{
	using var test = new TestDatabase();
	var customerId = await AddCustomerAsync(test);
	await AddStockAsync(test, productId: 1, quantity: 10);
	var service = test.CreateOrderService();

	var result = await service.PlaceSalesOrderAsync(customerId, Lines(1, 2));

	var invoice = await test.Db.Invoices.SingleAsync();
	Assert.Equal(result.OrderId, invoice.SalesOrderId);
	Assert.Equal($"INV-{invoice.IssueDate:yyyyMMdd}-{result.OrderId:D6}", invoice.InvoiceNumber);
	Assert.Equal(result.InvoiceNumber, invoice.InvoiceNumber);
	Assert.Equal(invoice.IssueDate.AddDays(30), invoice.DueDate);
	Assert.False(invoice.IsPaid);
}

[Fact]
public async Task Second_order_is_rejected_when_the_first_used_the_stock()
{
	using var test = new TestDatabase();
	var customerId = await AddCustomerAsync(test);
	await AddStockAsync(test, productId: 1, quantity: 5);
	var service = test.CreateOrderService();

	await service.PlaceSalesOrderAsync(customerId, Lines(1, 3)); // leaves 2

	var ex = await Assert.ThrowsAsync<InsufficientStockException>(
		() => service.PlaceSalesOrderAsync(customerId, Lines(1, 3)));

	Assert.Contains("available 2", ex.Message);
	Assert.Equal(1, await test.Db.SalesOrders.CountAsync());
}

[Fact]
public async Task Inactive_product_is_rejected()
{
	using var test = new TestDatabase();
	var customerId = await AddCustomerAsync(test);
	await AddStockAsync(test, productId: 8, quantity: 5); // product 8 is seeded as inactive
	var service = test.CreateOrderService();

	var ex = await Assert.ThrowsAsync<BusinessRuleException>(
		() => service.PlaceSalesOrderAsync(customerId, Lines(8, 1)));

	Assert.Contains("not active", ex.Message);
	Assert.Equal(0, await test.Db.SalesOrders.CountAsync());
}

[Fact]
public async Task Same_product_on_two_lines_is_combined_for_the_stock_check()
{
	using var test = new TestDatabase();
	var customerId = await AddCustomerAsync(test);
	await AddStockAsync(test, productId: 1, quantity: 5);
	var service = test.CreateOrderService();
	var lines = new List<OrderLineRequest>
	{
		new() { ProductId = 1, Quantity = 3 },
		new() { ProductId = 1, Quantity = 3 } // 6 in total, only 5 in stock
	};

	await Assert.ThrowsAsync<InsufficientStockException>(
		() => service.PlaceSalesOrderAsync(customerId, lines));

	Assert.Equal(0, await test.Db.SalesOrders.CountAsync());
}
	}
}