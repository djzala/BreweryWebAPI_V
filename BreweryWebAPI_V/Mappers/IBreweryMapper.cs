using BreweryWebAPI_V.Models;

namespace BreweryWebAPI_V.Mappers
{
    public interface IBreweryMapper
    {
        BreweryDto Map(OpenBreweryDto src);
    }
}
