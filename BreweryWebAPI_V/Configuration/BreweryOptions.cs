namespace BreweryWebAPI_V.Configuration
{
    public class BreweryOptions
    {
        public string OpenBreweryBaseUrl { get; set; } = "";
        public int PerPage { get; set; } = 100;
        public int CacheDurationMinutes { get; set; } = 10;
        public int HttpClientTimeoutSeconds { get; set; } = 10;
    }
}
