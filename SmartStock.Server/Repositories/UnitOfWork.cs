using Microsoft.EntityFrameworkCore.Storage;
using SmartStock.Server.Data;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AppDbContext _db;
		private IDbContextTransaction? _transaction;

		public UnitOfWork(AppDbContext db)
		{
			_db = db;

			// All repositories share the same DbContext, so one SaveChanges covers all of them
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

		public async Task BeginTransactionAsync(CancellationToken ct = default)
			=> _transaction = await _db.Database.BeginTransactionAsync(ct);

		public async Task CommitAsync(CancellationToken ct = default)
		{
			if (_transaction == null) return;

			await _transaction.CommitAsync(ct);
			await _transaction.DisposeAsync();
			_transaction = null;
		}

		public async Task RollbackAsync(CancellationToken ct = default)
		{
			if (_transaction == null) return;

			await _transaction.RollbackAsync(ct);
			await _transaction.DisposeAsync();
			_transaction = null;
		}

		public void Dispose()
		{
			_transaction?.Dispose();
		}
	}
}