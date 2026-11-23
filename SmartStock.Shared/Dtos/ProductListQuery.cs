namespace SmartStock.Shared.Dtos
{
	public class ProductListQuery
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;

		// Matches SKU or Name
		public string? Search { get; set; }

		public int? CategoryId { get; set; }

		// sku, name, category, price, stock
		public string SortBy { get; set; } = "name";
		public bool Descending { get; set; }
	}
}