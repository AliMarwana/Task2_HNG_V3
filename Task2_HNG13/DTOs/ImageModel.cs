namespace Task2_HNG13.DTOs
{
    public class ImageModel
    {
        public int TotalNumberOfCountries { get; set; }
        public List<String> TopFiveCountriesByGDP { get; set; }
        public DateTime LastRefresh { get; set; }

    }
}
