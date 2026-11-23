namespace SmartStock.Shared.Dtos
{
	public class ProductDto
	{
		public int Id { get; set; }
		public string SKU { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int CategoryId { get; set; }
		public bool IsActive { get; set; }
	}
}