using System.Linq.Expressions;

namespace SmartStock.Server.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

		Task<List<T>> GetAllAsync(CancellationToken ct = default);

		Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

		Task AddAsync(T entity, CancellationToken ct = default);

		void Update(T entity);

		void Remove(T entity);

		// Read-only query for projections (no tracking)
		IQueryable<T> Query();

		// Read-only: loads one row without tracking it
		Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default);
	}
}