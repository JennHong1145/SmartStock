using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public interface IProductService
	{
		Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default);

		Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);

		// Returns null if the SKU already exists
		Task<ProductDto?> CreateAsync(CreateProductRequest request, CancellationToken ct = default);

		// Returns false if the product doesn't exist
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);

		Task<PagedResult<ProductListDto>> GetPagedAsync(ProductListQuery query, CancellationToken ct = default);
	}
}