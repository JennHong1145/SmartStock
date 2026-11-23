using Microsoft.AspNetCore.Mvc;
using SmartStock.Server.Services;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryService _service;

		public CategoriesController(ICategoryService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
			=> Ok(await _service.GetAllAsync(ct));

		[HttpGet("{id:int}")]
		public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
		{
			var category = await _service.GetByIdAsync(id, ct);
			return category == null ? NotFound() : Ok(category);
		}

		[HttpPost]
		public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
		{
			var created = await _service.CreateAsync(request, ct);
			if (created == null) return Conflict("Category name already exists, or the parent category was not found.");

			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id, CancellationToken ct)
		{
			var result = await _service.DeleteAsync(id, ct);

			return result switch
			{
				DeleteCategoryResult.Deleted => NoContent(),
				DeleteCategoryResult.NotFound => NotFound(),
				_ => Conflict("This category still has products or sub-categories.")
			};
		}
	}
}