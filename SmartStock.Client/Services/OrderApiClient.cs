using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartStock.Shared.Dtos;

namespace SmartStock.Client.Services
{
	// Success value or a readable error, so pages never deal with raw HTTP codes
	public class ApiResult<T>
	{
		public T? Value { get; init; }
		public string? Error { get; init; }
		public HttpStatusCode StatusCode { get; init; }
		public bool Success => Error == null;
	}

	public class OrderApiClient
	{
		private readonly HttpClient _http;

		public OrderApiClient(HttpClient http)
		{
			_http = http;
		}

		public async Task<ApiResult<SalesOrderResultDto>> PlaceOrderAsync(PlaceSalesOrderRequest request)
		{
			HttpResponseMessage response;
			try
			{
				response = await _http.PostAsJsonAsync("api/Orders", request);
			}
			catch (HttpRequestException)
			{
				return new ApiResult<SalesOrderResultDto>
				{
					Error = "Could not reach the server. Is the API running?"
				};
			}

			if (response.IsSuccessStatusCode)
			{
				return new ApiResult<SalesOrderResultDto>
				{
					Value = await response.Content.ReadFromJsonAsync<SalesOrderResultDto>(),
					StatusCode = response.StatusCode
				};
			}

			var body = await response.Content.ReadAsStringAsync();
			return new ApiResult<SalesOrderResultDto>
			{
				Error = ReadMessage(body, response.StatusCode),
				StatusCode = response.StatusCode
			};
		}

		// Handles { "message": "..." }, validation ProblemDetails, or plain text
		private static string ReadMessage(string body, HttpStatusCode status)
		{
			try
			{
				using var doc = JsonDocument.Parse(body);
				var root = doc.RootElement;

				if (root.ValueKind == JsonValueKind.Object)
				{
					if (root.TryGetProperty("message", out var message))
						return message.GetString() ?? "Request failed.";

					if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
					{
						var lines = errors.EnumerateObject()
							.SelectMany(p => p.Value.EnumerateArray().Select(e => e.GetString()));
						return string.Join(" ", lines);
					}

					if (root.TryGetProperty("title", out var title))
						return title.GetString() ?? "Request failed.";
				}
			}
			catch (JsonException)
			{
				// Not JSON, fall through to the plain text below
			}

			return string.IsNullOrWhiteSpace(body) ? $"Request failed ({(int)status})." : body.Trim('"');
		}
	}
}