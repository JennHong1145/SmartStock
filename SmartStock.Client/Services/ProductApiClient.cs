using System.Net.Http.Json;
using SmartStock.Shared.Dtos;

namespace SmartStock.Client.Services
{
	// One class that knows how to talk to /api/Products, so pages never build URLs
	public class ProductApiClient
	{
		private readonly HttpClient _http;

		public ProductApiClient(HttpClient http)
		{
			_http = http;
		}

		public async Task<PagedResult<ProductListDto>> GetPagedAsync(ProductListQuery query)
		{
			var url = $"api/Products/paged?page={query.Page}&pageSize={query.PageSize}" +
					  $"&sortBy={Uri.EscapeDataString(query.SortBy)}&descending={query.Descending.ToString().ToLower()}";

			if (!string.IsNullOrWhiteSpace(query.Search))
				url += $"&search={Uri.EscapeDataString(query.Search)}";

			if (query.CategoryId.HasValue)
				url += $"&categoryId={query.CategoryId.Value}";

			var result = await _http.GetFromJsonAsync<PagedResult<ProductListDto>>(url);
			return result ?? new PagedResult<ProductListDto>();
		}
	}
}