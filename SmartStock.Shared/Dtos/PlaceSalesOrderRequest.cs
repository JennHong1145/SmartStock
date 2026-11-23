using System.ComponentModel.DataAnnotations;

namespace SmartStock.Shared.Dtos
{
	public class PlaceSalesOrderRequest
	{
		public int CustomerId { get; set; }

		[Required, MinLength(1)]
		public List<OrderLineRequest> Items { get; set; } = new();
	}
}