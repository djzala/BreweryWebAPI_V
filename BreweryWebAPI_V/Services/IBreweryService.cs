using BreweryWebAPI_V.Models;

namespace BreweryWebAPI_V.Services
{
    public interface IBreweryService
    {
        Task<IReadOnlyList<BreweryDto>> GetBreweriesAsync(string? q, string? sort, string? dir, double? lat, double? lon, int page, int pageSize, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetAutocompleteAsync(string prefix, int limit = 10, CancellationToken ct = default);
    }
}
