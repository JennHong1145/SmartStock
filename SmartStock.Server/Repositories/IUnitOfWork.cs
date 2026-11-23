using SmartStock.Server.Entities;

namespace SmartStock.Server.Repositories
{
	public interface IUnitOfWork
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

		// Runs the operation in one transaction, wrapped in the retry strategy
		Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken ct = default);

		// Current stock = sum of the ledger (see StockQueries)
		Task<int> GetCurrentStockAsync(int productId, CancellationToken ct = default);

		// Locks the product row until the transaction ends, so only one order at a time can sell it
		Task LockProductAsync(int productId, CancellationToken ct = default);
	}
}