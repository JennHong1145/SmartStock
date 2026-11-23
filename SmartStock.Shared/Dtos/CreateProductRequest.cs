using System.ComponentModel.DataAnnotations;

namespace SmartStock.Shared.Dtos
{
	public class CreateProductRequest
	{
		[Required, StringLength(100)]
		public string SKU { get; set; } = string.Empty;

		[Required, StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[Range(0, 1000000)]
		public decimal Price { get; set; }

		public int CategoryId { get; set; }
	}
}