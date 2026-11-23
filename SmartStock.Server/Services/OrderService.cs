using SmartStock.Server.Entities;
using SmartStock.Server.Repositories;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public class OrderService : IOrderService
	{
		private readonly IUnitOfWork _uow;

		public OrderService(IUnitOfWork uow)
		{
			_uow = uow;
		}

		public async Task<SalesOrderResultDto> PlaceSalesOrderAsync(
			int customerId, List<OrderLineRequest> items, CancellationToken ct = default)
		{
			if (items.Count == 0)
				throw new BusinessRuleException("An order needs at least one line.");

			// The same product on two lines counts as one for the stock check
			var lines = items
				.GroupBy(i => i.ProductId)
				.Select(g => new OrderLineRequest { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
				.OrderBy(l => l.ProductId)
				.ToList();

			return await _uow.ExecuteInTransactionAsync(async () =>
			{
				var customer = await _uow.Customers.GetByIdNoTrackingAsync(customerId, ct);
				if (customer == null)
					throw new BusinessRuleException($"Customer {customerId} was not found.");

				var now = DateTime.UtcNow;
				var order = new SalesOrder
				{
					CustomerId = customerId,
					OrderDate = now,
					Status = SalesOrderStatus.Confirmed
				};

				// 1. Validate every line (stock guard) and build the order lines
				foreach (var line in lines)
				{
					if (line.Quantity <= 0)
						throw new BusinessRuleException("Quantity must be more than 0.");

					var product = await _uow.Products.GetByIdNoTrackingAsync(line.ProductId, ct);
					if (product == null || !product.IsActive)
						throw new BusinessRuleException($"Product {line.ProductId} was not found or is not active.");

					await _uow.LockProductAsync(product.Id, ct);

					var available = await _uow.GetCurrentStockAsync(product.Id, ct);
					if (line.Quantity > available)
						throw new InsufficientStockException(product.SKU, line.Quantity, available);

					// The line keeps its own price, so later price changes don't change this order
					order.Items.Add(new SalesOrderItem
					{
						ProductId = product.Id,
						Quantity = line.Quantity,
						UnitPrice = product.Price
					});
				}

				order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

				// 2. Save the order + lines first, so the order gets its Id
				await _uow.SalesOrders.AddAsync(order, ct);
				await _uow.SaveChangesAsync(ct);

				// 3. One SaleOut movement per line, pointing back to the order
				foreach (var item in order.Items)
				{
					await _uow.StockMovements.AddAsync(new StockMovement
					{
						ProductId = item.ProductId,
						MovementType = StockMovementType.SaleOut,
						Quantity = item.Quantity,
						MovementDate = now,
						ReferenceId = order.Id,
						Notes = $"Sales order #{order.Id}"
					}, ct);
				}

				// 4. The invoice (order Id makes the number unique)
				var invoice = new Invoice
				{
					SalesOrderId = order.Id,
					InvoiceNumber = $"INV-{now:yyyyMMdd}-{order.Id:D6}",
					IssueDate = now,
					DueDate = now.AddDays(30),
					IsPaid = false
				};
				await _uow.Invoices.AddAsync(invoice, ct);

				await _uow.SaveChangesAsync(ct);

				return new SalesOrderResultDto
				{
					OrderId = order.Id,
					InvoiceNumber = invoice.InvoiceNumber,
					TotalAmount = order.TotalAmount
				};
			}, ct);
		}
	}
}