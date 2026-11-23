using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Data;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public interface IReportService
	{
		Task<List<SalesSummaryDto>> GetSalesSummaryAsync(CancellationToken ct = default);

		Task<List<SalesSummaryDto>> GetSalesSummaryByDateAsync(DateTime from, DateTime to, CancellationToken ct = default);
	}

	public class ReportService : IReportService
	{
		private readonly AppDbContext _db;

		public ReportService(AppDbContext db)
		{
			_db = db;
		}

		// Reads the view like any other DbSet
		public async Task<List<SalesSummaryDto>> GetSalesSummaryAsync(CancellationToken ct = default)
		{
			return await _db.SalesSummaryByProducts
				.OrderByDescending(s => s.TotalRevenue)
				.Select(s => new SalesSummaryDto
				{
					ProductId = s.ProductId,
					ProductName = s.ProductName,
					TotalQuantitySold = s.TotalQuantitySold,
					TotalRevenue = s.TotalRevenue
				})
				.ToListAsync(ct);
		}

		// Same report for a date range. The view has no date column, so this uses raw SQL.
		public async Task<List<SalesSummaryDto>> GetSalesSummaryByDateAsync(DateTime from, DateTime to, CancellationToken ct = default)
		{
			// FromSqlInterpolated turns {from} and {to} into SQL parameters, not text.
			// The query has no ORDER BY inside, so EF can add its own on top.
			return await _db.SalesSummaryByProducts
				.FromSqlInterpolated($@"
SELECT
	p.Id AS ProductId,
	p.Name AS ProductName,
	SUM(i.Quantity) AS TotalQuantitySold,
	SUM(i.LineTotal) AS TotalRevenue
FROM SalesOrderItems AS i
INNER JOIN SalesOrders AS o ON o.Id = i.SalesOrderId
INNER JOIN Products AS p ON p.Id = i.ProductId
WHERE o.Status IN ('Confirmed', 'Shipped')
	AND o.OrderDate >= {from}
	AND o.OrderDate < {to}
GROUP BY p.Id, p.Name")
				.OrderByDescending(s => s.TotalRevenue)
				.Select(s => new SalesSummaryDto
				{
					ProductId = s.ProductId,
					ProductName = s.ProductName,
					TotalQuantitySold = s.TotalQuantitySold,
					TotalRevenue = s.TotalRevenue
				})
				.ToListAsync(ct);
		}
	}
}