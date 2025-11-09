namespace BreweryWebAPI_V.Models
{
    public class BreweryDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public double? DistanceKm { get; set; } // set after compute
    }
}
