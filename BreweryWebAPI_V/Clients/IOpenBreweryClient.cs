using BreweryWebAPI_V.Models;

namespace BreweryWebAPI_V.Clients
{
    public interface IOpenBreweryClient
    {
        Task<IReadOnlyList<OpenBreweryDto>> FetchBreweriesAsync(int perPage, CancellationToken ct = default);
    }
}
