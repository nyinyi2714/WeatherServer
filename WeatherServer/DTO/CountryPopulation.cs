namespace WeatherServer.DTO
{
    public class CountryPopulation
    {
        public required string Name { get; set; }
        public int CountryId { get; set; }
        public int Population { get; set; }
    }
}
