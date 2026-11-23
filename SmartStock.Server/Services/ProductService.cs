using SmartStock.Server.Entities;
using SmartStock.Server.Repositories;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public class ProductService : IProductService
	{
		private readonly IUnitOfWork _uow;

		public ProductService(IUnitOfWork uow)
		{
			_uow = uow;
		}

		public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default)
		{
			var products = await _uow.Products.GetAllAsync(ct);
			return products.Select(ToDto).ToList();
		}

		public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
		{
			var product = await _uow.Products.GetByIdAsync(id, ct);
			return product == null ? null : ToDto(product);
		}

		public async Task<ProductDto?> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
		{
			// Business rule: SKU must be unique
			var existing = await _uow.Products.FindAsync(p => p.SKU == request.SKU, ct);
			if (existing.Count > 0) return null;

			var product = new Product
			{
				SKU = request.SKU,
				Name = request.Name,
				Price = request.Price,
				CategoryId = request.CategoryId,
				IsActive = true
			};

			await _uow.Products.AddAsync(product, ct);
			await _uow.SaveChangesAsync(ct);

			return ToDto(product);
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{
			var product = await _uow.Products.GetByIdAsync(id, ct);
			if (product == null) return false;

			// Soft delete: AppDbContext turns this into IsDeleted = true
			_uow.Products.Remove(product);
			await _uow.SaveChangesAsync(ct);
			return true;
		}

		private static ProductDto ToDto(Product p) => new()
		{
			Id = p.Id,
			SKU = p.SKU,
			Name = p.Name,
			Price = p.Price,
			CategoryId = p.CategoryId,
			IsActive = p.IsActive
		};
	}
}