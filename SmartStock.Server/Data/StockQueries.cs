using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Entities;

namespace SmartStock.Server.Data
{
	public static class StockQueries
	{
		public static async Task<int> GetCurrentStockAsync(AppDbContext db, int productId)
		{
			return await db.StockMovements
				.AsNoTracking()
				.Where(m => m.ProductId == productId)
				.SumAsync(m => m.MovementType == StockMovementType.SaleOut
					? -m.Quantity
					: m.Quantity);
		}
	}
}