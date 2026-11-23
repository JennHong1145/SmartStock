using Microsoft.AspNetCore.Mvc;
using SmartStock.Server.Services;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductService _service;

		public ProductsController(IProductService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken ct)
			=> Ok(await _service.GetAllAsync(ct));

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
		{
			var product = await _service.GetByIdAsync(id, ct);
			return product == null ? NotFound() : Ok(product);
		}

		[HttpPost]
		public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
		{
			var created = await _service.CreateAsync(request, ct);
			if (created == null) return Conflict("A product with this SKU already exists.");

			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id, CancellationToken ct)
			=> await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
	}
}