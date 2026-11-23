namespace SmartStock.Shared.Dtos
{
	public class SalesSummaryDto
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public int TotalQuantitySold { get; set; }
		public decimal TotalRevenue { get; set; }
	}
}