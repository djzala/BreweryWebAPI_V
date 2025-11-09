using BreweryWebAPI_V.Clients;
using BreweryWebAPI_V.Configuration;
using BreweryWebAPI_V.Exceptions;
using BreweryWebAPI_V.Mappers;
using BreweryWebAPI_V.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BreweryWebAPI_V.Services
{
    public class BreweryService : IBreweryService
    {
        private readonly IOpenBreweryClient _client;
        private readonly IMemoryCache _cache;
        private readonly BreweryOptions _options;
        private readonly IBreweryMapper _mapper;
        private readonly ILogger<BreweryService> _logger;
        private static readonly string CacheKey = "OpenBrewery_All";

        public BreweryService(IOpenBreweryClient client, IMemoryCache cache, IOptions<BreweryOptions> opts, IBreweryMapper mapper, ILogger<BreweryService> logger)
        {
            _client = client;
            _cache = cache;
            _options = opts.Value;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<BreweryDto>> GetBreweriesAsync(string? q, string? sort, string? dir, double? lat, double? lon, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) throw new ApiException("page must be >= 1", 400);
            if (pageSize <= 0 || pageSize > 500) throw new ApiException("pageSize must be between 1 and 500", 400);

            var raw = await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
                entry.Priority = CacheItemPriority.High;
                _logger.LogInformation("Cache miss; fetching upstream breweries");
                var result = await _client.FetchBreweriesAsync(_options.PerPage, ct);
                return result;
            });

            var mapped = raw.Select(r => _mapper.Map(r)).ToList();

            // compute distances if lat/lon provided
            if (lat.HasValue && lon.HasValue)
            {
                foreach (var b in mapped)
                {
                    if (b.Latitude.HasValue && b.Longitude.HasValue)
                        b.DistanceKm = HaversineKm(lat.Value, lon.Value, b.Latitude.Value, b.Longitude.Value);
                    else
                        b.DistanceKm = null;
                }
            }

            // search by name or city
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.Trim().ToLowerInvariant();
                mapped = mapped.Where(b =>
                    (!string.IsNullOrEmpty(b.Name) && b.Name.ToLowerInvariant().Contains(qLower)) ||
                    (!string.IsNullOrEmpty(b.City) && b.City.ToLowerInvariant().Contains(qLower))).ToList();
            }

            // sort
            mapped = ApplySort(mapped, sort, dir, lat, lon);

            // paging
            var skip = (page - 1) * pageSize;
            var paged = mapped.Skip(skip).Take(pageSize).ToList();

            return paged;
        }

        public async Task<IReadOnlyList<string>> GetAutocompleteAsync(string prefix, int limit = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return Array.Empty<string>();

            var raw = await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
                _logger.LogInformation("Cache miss (autocomplete); fetching upstream breweries");
                var result = await _client.FetchBreweriesAsync(_options.PerPage, ct);
                return result;
            });

            var mapped = raw.Select(r => _mapper.Map(r));
            var results = mapped
                .Where(b => !string.IsNullOrWhiteSpace(b.Name) && b.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(b => b.Name)
                .Distinct()
                .Take(limit)
                .ToList();

            return results;
        }

        private static List<BreweryDto> ApplySort(List<BreweryDto> list, string? sort, string? dir, double? lat, double? lon)
        {
            var ascending = string.IsNullOrEmpty(dir) || dir.Equals("asc", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sort))
                return ascending ? list.OrderBy(b => b.Name).ToList() : list.OrderByDescending(b => b.Name).ToList();

            switch (sort.ToLowerInvariant())
            {
                case "name":
                    return ascending ? list.OrderBy(b => b.Name).ToList() : list.OrderByDescending(b => b.Name).ToList();
                case "city":
                    return ascending ? list.OrderBy(b => b.City).ToList() : list.OrderByDescending(b => b.City).ToList();
                case "distance":
                    if (!lat.HasValue || !lon.HasValue)
                        throw new ApiException("lat and lon must be provided when sorting by distance", 400);
                    return ascending ? list.OrderBy(b => b.DistanceKm ?? double.MaxValue).ToList() : list.OrderByDescending(b => b.DistanceKm ?? double.MinValue).ToList();
                default:
                    return ascending ? list.OrderBy(b => b.Name).ToList() : list.OrderByDescending(b => b.Name).ToList();
            }
        }

        // Haversine formula
        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371;
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private static double ToRad(double deg) => deg * (Math.PI / 180.0);
    }
}
