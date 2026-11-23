using Microsoft.AspNetCore.Mvc;
using SmartStock.Server.Services;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReportsController : ControllerBase
	{
		private readonly IReportService _service;

		public ReportsController(IReportService service)
		{
			_service = service;
		}

		[HttpGet("sales-summary")]
		public async Task<ActionResult<List<SalesSummaryDto>>> SalesSummary(CancellationToken ct)
			=> Ok(await _service.GetSalesSummaryAsync(ct));

		[HttpGet("sales-summary/range")]
		public async Task<ActionResult<List<SalesSummaryDto>>> SalesSummaryByDate(
			[FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
		{
			if (to <= from) return BadRequest(new { message = "'to' must be after 'from'." });

			return Ok(await _service.GetSalesSummaryByDateAsync(from, to, ct));
		}
	}
}