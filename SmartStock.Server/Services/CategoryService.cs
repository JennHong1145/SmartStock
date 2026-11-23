using SmartStock.Server.Entities;
using SmartStock.Server.Repositories;
using SmartStock.Shared.Dtos;

namespace SmartStock.Server.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly IUnitOfWork _uow;

		public CategoryService(IUnitOfWork uow)
		{
			_uow = uow;
		}

		public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
		{
			var categories = await _uow.Categories.GetAllAsync(ct);
			return categories.Select(ToDto).ToList();
		}

		public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
		{
			var category = await _uow.Categories.GetByIdAsync(id, ct);
			return category == null ? null : ToDto(category);
		}

		public async Task<CategoryDto?> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
		{
			// Business rule: category names must be unique
			var sameName = await _uow.Categories.FindAsync(c => c.Name == request.Name, ct);
			if (sameName.Count > 0) return null;

			// Business rule: the parent category must exist
			if (request.ParentCategoryId != null)
			{
				var parent = await _uow.Categories.GetByIdAsync(request.ParentCategoryId.Value, ct);
				if (parent == null) return null;
			}

			var category = new Category
			{
				Name = request.Name,
				Description = request.Description,
				ParentCategoryId = request.ParentCategoryId
			};

			await _uow.Categories.AddAsync(category, ct);
			await _uow.SaveChangesAsync(ct);

			return ToDto(category);
		}

		public async Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken ct = default)
		{
			var category = await _uow.Categories.GetByIdAsync(id, ct);
			if (category == null) return DeleteCategoryResult.NotFound;

			// Check first, so the user gets a clear message instead of a database error
			var hasProducts = (await _uow.Products.FindAsync(p => p.CategoryId == id, ct)).Count > 0;
			var hasSubCategories = (await _uow.Categories.FindAsync(c => c.ParentCategoryId == id, ct)).Count > 0;
			if (hasProducts || hasSubCategories) return DeleteCategoryResult.HasProductsOrSubCategories;

			_uow.Categories.Remove(category);
			await _uow.SaveChangesAsync(ct);
			return DeleteCategoryResult.Deleted;
		}

		private static CategoryDto ToDto(Category c) => new()
		{
			Id = c.Id,
			Name = c.Name,
			Description = c.Description,
			ParentCategoryId = c.ParentCategoryId
		};
	}
}