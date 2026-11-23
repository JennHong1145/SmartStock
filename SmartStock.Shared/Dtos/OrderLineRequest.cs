using System.ComponentModel.DataAnnotations;

namespace SmartStock.Shared.Dtos
{
	public class OrderLineRequest
	{
		public int ProductId { get; set; }

		[Range(1, 100000)]
		public int Quantity { get; set; }
	}
}