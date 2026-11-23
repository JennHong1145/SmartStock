using Microsoft.AspNetCore.Mvc;
using SmartStock.Server.Services;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderService _service;

		public OrdersController(IOrderService service)
		{
			_service = service;
		}

		[HttpPost]
		public async Task<ActionResult<SalesOrderResultDto>> Place(PlaceSalesOrderRequest request, CancellationToken ct)
		{
			try
			{
				var result = await _service.PlaceSalesOrderAsync(request.CustomerId, request.Items, ct);
				return StatusCode(StatusCodes.Status201Created, result);
			}
			catch (BusinessRuleException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}