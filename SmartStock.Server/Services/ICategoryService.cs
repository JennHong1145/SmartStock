using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public interface ICategoryService
	{
		Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default);

		Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);

		// Returns null if the name already exists or the parent category doesn't exist
		Task<CategoryDto?> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);

		Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken ct = default);
	}

	public enum DeleteCategoryResult
	{
		Deleted,
		NotFound,
		HasProductsOrSubCategories
	}
}