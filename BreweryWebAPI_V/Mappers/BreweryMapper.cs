using BreweryWebAPI_V.Models;
using System.Globalization;

namespace BreweryWebAPI_V.Mappers
{
    public class BreweryMapper : IBreweryMapper
    {
        public BreweryDto Map(OpenBreweryDto src)
        {
            double? lat = TryParseDouble(src.latitude);
            double? lon = TryParseDouble(src.longitude);

            return new BreweryDto
            {
                Id = src.id ?? Guid.NewGuid().ToString(),
                Name = src.name ?? string.Empty,
                City = src.city ?? string.Empty,
                Phone = src.phone,
                Latitude = lat,
                Longitude = lon,
                DistanceKm = null
            };
        }

        private static double? TryParseDouble(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return d;
            if (double.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return d;
            return null;
        }
    }
}
