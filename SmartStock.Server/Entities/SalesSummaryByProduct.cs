namespace SmartStock.Server.Entities
{
	// Keyless: one row per product, read from the view vw_SalesSummaryByProduct.
	// There is no table behind it, so it has no Id and can't be saved.
	public class SalesSummaryByProduct
	{
		public int ProductId { get; set; }

		public string ProductName { get; set; } = string.Empty;

		public int TotalQuantitySold { get; set; }

		public decimal TotalRevenue { get; set; }
	}
}