using SmartStock.Server.Entities;

namespace SmartStock.Server.Repositories
{
	public interface IUnitOfWork : IDisposable
	{
		IRepository<Category> Categories { get; }
		IRepository<Product> Products { get; }
		IRepository<Supplier> Suppliers { get; }
		IRepository<Customer> Customers { get; }
		IRepository<PurchaseOrder> PurchaseOrders { get; }
		IRepository<SalesOrder> SalesOrders { get; }
		IRepository<Invoice> Invoices { get; }
		IRepository<StockMovement> StockMovements { get; }

		Task<int> SaveChangesAsync(CancellationToken ct = default);

		Task BeginTransactionAsync(CancellationToken ct = default);
		Task CommitAsync(CancellationToken ct = default);
		Task RollbackAsync(CancellationToken ct = default);
	}
}