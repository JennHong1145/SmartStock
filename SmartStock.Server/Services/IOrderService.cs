using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public interface IOrderService
	{
		Task<SalesOrderResultDto> PlaceSalesOrderAsync(int customerId, List<OrderLineRequest> items, CancellationToken ct = default);
	}
}