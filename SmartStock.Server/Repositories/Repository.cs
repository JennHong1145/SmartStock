using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Data;

namespace SmartStock.Server.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		protected readonly AppDbContext _db;
		protected readonly DbSet<T> _set;

		public Repository(AppDbContext db)
		{
			_db = db;
			_set = db.Set<T>();
		}

		public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
			=> await _set.FindAsync(new object[] { id }, ct);

		public async Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default)
			=> await _set.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

		// AsNoTracking: read-only lists don't need change tracking, so they're faster
		public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
			=> await _set.AsNoTracking().ToListAsync(ct);

		public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
			=> await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

		public async Task AddAsync(T entity, CancellationToken ct = default)
			=> await _set.AddAsync(entity, ct);

		public void Update(T entity) => _set.Update(entity);

		public void Remove(T entity) => _set.Remove(entity);

		public IQueryable<T> Query() => _set.AsNoTracking();
	}
}