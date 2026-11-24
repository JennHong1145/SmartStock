using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Data;
using SmartStock.Server.Repositories;
using SmartStock.Server.Services;

namespace SmartStock.Tests.Infrastructure
{
	// One fresh in-memory database per test. It lives only while the connection is open.
	public sealed class TestDatabase : IDisposable
	{
		private readonly SqliteConnection _connection;

		public TestDbContext Db { get; }

		public TestDatabase()
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			var options = new DbContextOptionsBuilder<AppDbContext>()
				.UseSqlite(_connection)
				.Options;

			Db = new TestDbContext(options);
			Db.Database.EnsureCreated(); // builds the tables and inserts the HasData rows
		}

		public IUnitOfWork CreateUnitOfWork() => new UnitOfWork(Db);

		public IOrderService CreateOrderService() => new OrderService(CreateUnitOfWork());

		public void Dispose()
		{
			Db.Dispose();
			_connection.Dispose();
		}
	}
}