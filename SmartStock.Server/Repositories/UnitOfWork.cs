using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Data;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AppDbContext _db;

		public UnitOfWork(AppDbContext db)
		{
			_db = db;

			Categories = new Repository<Category>(db);
			Products = new Repository<Product>(db);
			Suppliers = new Repository<Supplier>(db);
			Customers = new Repository<Customer>(db);
			PurchaseOrders = new Repository<PurchaseOrder>(db);
			SalesOrders = new Repository<SalesOrder>(db);
			Invoices = new Repository<Invoice>(db);
			StockMovements = new Repository<StockMovement>(db);
		}

		public IRepository<Category> Categories { get; }
		public IRepository<Product> Products { get; }
		public IRepository<Supplier> Suppliers { get; }
		public IRepository<Customer> Customers { get; }
		public IRepository<PurchaseOrder> PurchaseOrders { get; }
		public IRepository<SalesOrder> SalesOrders { get; }
		public IRepository<Invoice> Invoices { get; }
		public IRepository<StockMovement> StockMovements { get; }

		public Task<int> SaveChangesAsync(CancellationToken ct = default)
			=> _db.SaveChangesAsync(ct);

		public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken ct = default)
		{
			var strategy = _db.Database.CreateExecutionStrategy();

			// On a transient SQL error the strategy runs this whole block again from the start
			return strategy.ExecuteAsync(async () =>
			{
				// If we leave without Commit, disposing the transaction rolls everything back
				await using var tx = await _db.Database.BeginTransactionAsync(ct);
				try
				{
					var result = await operation();
					await tx.CommitAsync(ct);
					return result;
				}
				catch
				{
					// Forget half-built entities, so a retry starts clean
					_db.ChangeTracker.Clear();
					throw;
				}
			});
		}

		public Task<int> GetCurrentStockAsync(int productId, CancellationToken ct = default)
			=> StockQueries.GetCurrentStockAsync(_db, productId);

		public async Task LockProductAsync(int productId, CancellationToken ct = default)
{
	// UPDLOCK + HOLDLOCK is SQL Server syntax. Other providers (like SQLite in tests)
	// don't support it, so only run it on SQL Server.
	if (!_db.Database.IsSqlServer())
		return;

	// Others must wait here until this transaction commits or rolls back
	await _db.Database.ExecuteSqlInterpolatedAsync(
		$"SELECT Id FROM Products WITH (UPDLOCK, HOLDLOCK) WHERE Id = {productId}", ct);
}
	}
}