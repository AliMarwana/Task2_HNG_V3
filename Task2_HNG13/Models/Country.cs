using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task2_HNG13.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name{ get; set; }
        public string? Capital{ get; set; }
        public string? Region{ get; set; }
        private int? _population;
        public int? Population { 
            get {
                return _population; 
            }
            set {
            
                _population = value;
                GetEstimatedGdp();
            }
             }
        public string? Currency_code { get; set; }
        private int? _exchange_rate;
        public int? Exchange_rate{ get {
                return _exchange_rate;
            } 
            set { 
                _exchange_rate = value;
                GetEstimatedGdp();
            } }


        private float _estimated_gdp;
        public float Estimated_gdp { get { 
                return _estimated_gdp;
            }
            set {

                _estimated_gdp = value;   
            }
             }
        public string? Flag_url { get; set; }
        public DateTime Last_refreshed_at { get; set; } = DateTime.UtcNow;


        //Here we calculate estimated gdp based on population and exchange rate
        private void GetEstimatedGdp()
        {
            if (Population <= 0 || Exchange_rate <= 0)
            {
                _estimated_gdp = 0;
            }
            else if (Population == null || Exchange_rate == null)
            {
                _estimated_gdp = 0;
            }
            else
            {
                Random random = new Random();

                // Generate random number between 1000 and 2000 (inclusive)
                int randomNumber = random.Next(1000, 2001);
                _estimated_gdp = (float)Math.Round((decimal)(Population * randomNumber / Exchange_rate), 1);
            }
        }
    }
}
