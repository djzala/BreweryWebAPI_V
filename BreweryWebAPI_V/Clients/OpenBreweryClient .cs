using BreweryWebAPI_V.Models;
using System.Text.Json;

namespace BreweryWebAPI_V.Clients
{
    public class OpenBreweryClient : IOpenBreweryClient
    {
        private readonly HttpClient _http;

        public OpenBreweryClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<OpenBreweryDto>> FetchBreweriesAsync(int perPage, CancellationToken ct = default)
        {
            var url = $"/breweries?per_page={perPage}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Upstream returned {(int)resp.StatusCode} {resp.ReasonPhrase}");

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<List<OpenBreweryDto>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, ct);

            return data ?? new List<OpenBreweryDto>();
        }
    }
}
