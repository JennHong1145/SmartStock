using Microsoft.EntityFrameworkCore;
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
			var product = await _uow.Products.GetByIdNoTrackingAsync(id, ct);
			return product == null ? null : ToDto(product);
		}

public async Task<ProductDto?> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
{
	// Business rules that don't depend on the DTO attributes
	if (request.Price < 0)
		throw new BusinessRuleException("Price must be 0 or more.");

	var categoryExists = await _uow.Categories.Query().AnyAsync(c => c.Id == request.CategoryId, ct);
	if (!categoryExists)
		throw new BusinessRuleException($"Category {request.CategoryId} does not exist.");

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

		public async Task<PagedResult<ProductListDto>> GetPagedAsync(ProductListQuery q, CancellationToken ct = default)
		{
			// Keep the page numbers sensible
			var page = Math.Max(1, q.Page);
			var pageSize = Math.Clamp(q.PageSize, 1, 100);

			var products = _uow.Products.Query();
			var categories = _uow.Categories.Query();
			var movements = _uow.StockMovements.Query();

			// Filter first, on the Products table, so SQL Server does less work
			if (!string.IsNullOrWhiteSpace(q.Search))
			{
				var s = q.Search.Trim();
				products = products.Where(p => p.Name.Contains(s) || p.SKU.Contains(s));
			}

			if (q.CategoryId != null)
				products = products.Where(p => p.CategoryId == q.CategoryId);

			var totalCount = await products.CountAsync(ct);

			// One query: category name and stock are worked out inside SQL
			var list =
				from p in products
				join c in categories on p.CategoryId equals c.Id
				select new ProductListDto
				{
					Id = p.Id,
					SKU = p.SKU,
					Name = p.Name,
					CategoryName = c.Name,
					Price = p.Price,
					CurrentStock = movements
						.Where(m => m.ProductId == p.Id)
						.Sum(m => m.MovementType == StockMovementType.SaleOut ? -m.Quantity : m.Quantity)
				};

			// Sorting happens in SQL too. Id is the tie-breaker so pages never overlap.
			list = (q.SortBy.ToLowerInvariant(), q.Descending) switch
			{
				("sku", false) => list.OrderBy(x => x.SKU).ThenBy(x => x.Id),
				("sku", true) => list.OrderByDescending(x => x.SKU).ThenBy(x => x.Id),
				("category", false) => list.OrderBy(x => x.CategoryName).ThenBy(x => x.Id),
				("category", true) => list.OrderByDescending(x => x.CategoryName).ThenBy(x => x.Id),
				("price", false) => list.OrderBy(x => x.Price).ThenBy(x => x.Id),
				("price", true) => list.OrderByDescending(x => x.Price).ThenBy(x => x.Id),
				("stock", false) => list.OrderBy(x => x.CurrentStock).ThenBy(x => x.Id),
				("stock", true) => list.OrderByDescending(x => x.CurrentStock).ThenBy(x => x.Id),
				(_, true) => list.OrderByDescending(x => x.Name).ThenBy(x => x.Id),
				_ => list.OrderBy(x => x.Name).ThenBy(x => x.Id)
			};

			var items = await list
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync(ct);

			return new PagedResult<ProductListDto>
			{
				Items = items,
				TotalCount = totalCount,
				Page = page,
				PageSize = pageSize
			};
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