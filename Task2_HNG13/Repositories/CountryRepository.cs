using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using Task2_HNG13.Data;
using Task2_HNG13.DTOs;
using Task2_HNG13.Models;

namespace Task2_HNG13.Repositories
{
    public class CountryRepository
    {
        private AppDbContext _context;
        private HttpClient _client;
        //private readonly string cacheDirectory = Path.Combine(AppDomain.CurrentDomain., "cache");
        private readonly IWebHostEnvironment _environment;
        public CountryRepository(AppDbContext context, HttpClient client, 
            IWebHostEnvironment environment)
        {
            _context = context;
            _client = client;
            _environment = environment;
        }
       

        public async Task GenerateImage(List<Country> countries, DateTime last_update)
        {
            var imageModel = new ImageModel();
            imageModel.TotalNumberOfCountries = countries.Count();
            imageModel.TopFiveCountriesByGDP = countries.OrderByDescending(p => p.Estimated_gdp).Take(5).Select(p =>
            {
                var countryname = p.Name;
                return countryname;
            }).ToList();
            imageModel.LastRefresh = last_update;

            // Your data
        
            DateTime timeStamp = DateTime.Now;
            string contentRootPath = Path.Combine(_environment.ContentRootPath, "cache");
            string cacheKey = $"countries_report.png";
            string cacheFilePath = Path.Combine(contentRootPath, cacheKey);

            EnsureCacheDirectoryExists(contentRootPath);
            string sanitizedFileName = Path.GetFileName(cacheKey);
            string fullPath = Path.Combine(cacheFilePath, sanitizedFileName);

            // Security check: ensure the file is within the intended directory
            // Check if file exists and has valid image extension
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            // Generate the image
            var imageBytes = GenerateAndCacheImage(imageModel.TotalNumberOfCountries, 
                imageModel.TopFiveCountriesByGDP, last_update, cacheFilePath);

            // Return as image response
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new ByteArrayContent(imageBytes);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
            {
                FileName = "countries-report.png"
            };

        }
        private void EnsureCacheDirectoryExists(string rootPathDirectory)
        {
            if (!Directory.Exists(rootPathDirectory))
            {
                Directory.CreateDirectory(rootPathDirectory);
            }
        }
        private byte[] GenerateAndCacheImage(int totalCountries, List<string> countries, DateTime timeStamp, string cacheFilePath)
        {
            // Generate the image
            byte[] imageBytes = GenerateCountriesImage(totalCountries, countries, timeStamp);

            // Save to cache directory
            File.WriteAllBytes(cacheFilePath, imageBytes);

            return imageBytes;
        }

        private byte[] GenerateCountriesImage(int totalCountries, List<string> countries, DateTime timeStamp)
        {
            int width = 600;
            int height = 400;

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var memoryStream = new MemoryStream())
            {
                // Set background color
                graphics.Clear(Color.White);

                // Set up fonts and brushes
                var titleFont = new Font("Arial", 20, FontStyle.Bold);
                var headerFont = new Font("Arial", 16, FontStyle.Bold);
                var contentFont = new Font("Arial", 12);
                var timestampFont = new Font("Arial", 10, FontStyle.Italic);

                var blackBrush = new SolidBrush(Color.Black);
                var blueBrush = new SolidBrush(Color.Blue);
                var darkRedBrush = new SolidBrush(Color.DarkRed);

                // Draw title
                string title = "Countries Report";
                var titleSize = graphics.MeasureString(title, titleFont);
                graphics.DrawString(title, titleFont, blueBrush, (width - titleSize.Width) / 2, 20);

                // Draw total countries
                string totalText = $"Total Number of Countries: {totalCountries}";
                graphics.DrawString(totalText, headerFont, darkRedBrush, 50, 70);

                // Draw countries list
                graphics.DrawString("Top 5 Countries by Population:", headerFont, blackBrush, 50, 110);

                int yPosition = 140;
                for (int i = 0; i < countries.Count; i++)
                {
                    string countryText = $"{i + 1}. {countries[i]}";
                    graphics.DrawString(countryText, contentFont, blackBrush, 70, yPosition);
                    yPosition += 25;
                }

                // Draw timestamp
                string timestampText = $"Generated on: {timeStamp:yyyy-MM-dd HH:mm:ss}";
                var timestampSize = graphics.MeasureString(timestampText, timestampFont);
                graphics.DrawString(timestampText, timestampFont, blackBrush, width - timestampSize.Width - 10, height - 30);

                // Draw border
                graphics.DrawRectangle(new Pen(Color.Gray, 2), 10, 10, width - 20, height - 20);

                // Save to memory stream and return bytes
                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }
        public async Task<JToken> GetExchangeRates()
        {
            try
            {
                var response = await _client.GetAsync("https://open.er-api.com/v6/latest/USD");
                if(response.IsSuccessStatusCode)
                {
                   var content = await response.Content.ReadAsStringAsync();
                   var JSONObject = JObject.Parse(content);
                    var exchangeRates = JSONObject["rates"];
                    //var exchangeRates = JsonSerializer.Deserialize<dynamic>(content).rates;
                   return exchangeRates;
                    
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                }

            }
            catch(Exception ex)
            {
                throw;
         
            }

            //return await Task.FromResult(_context.Countries.ToList());
        }
        public async Task SaveCountriesAfterCheckInDb(List<Country> countriesFromAPI, DateTime last_update)
        {
           
            var countriesFromDb = new List<Country>();
            foreach (var country in countriesFromAPI)
            {
                var countryInDb = await _context.Countries.AsNoTracking().FirstOrDefaultAsync(p => p.Name == country.Name);
                if(countryInDb == null)
                {
                    country.Last_refreshed_at = last_update;
                    await _context.Countries.AddAsync(country);
                    await _context.SaveChangesAsync();
                    countriesFromDb.Add(country);
                    
                }
                else
                {
                    countryInDb.Name = country.Name;
                    countryInDb.Capital = country.Capital;
                    countryInDb.Region = country.Region;
                    countryInDb.Population = country.Population;
                    countryInDb.Currency_code = country.Currency_code;
                    countryInDb.Exchange_rate = country.Exchange_rate;
                    countryInDb.Flag_url = country.Flag_url;
                   countryInDb.Estimated_gdp = country.Estimated_gdp;
                    countryInDb.Last_refreshed_at = last_update;
                }
               
            }
            await _context.SaveChangesAsync();

            //return countriesFromAPI;    
        }
        
        public Dictionary<string, string> IsValidationSuccess(List<Country> countriesFromAPI)
        {
            var isSuccessful = true;
            var errors = new Dictionary<string, string>();
            foreach (var country in countriesFromAPI)
            {
                if(String.IsNullOrEmpty(country.Name))
                {
                    errors.Add("name", "is required");
                }
                if(country.Population == null)
                {
                    errors.Add("population", "is required");
                }
                if(String.IsNullOrEmpty(country.Currency_code))
                {
                    errors.Add("currency_code", "is required");
                }
                break;
            }
            return errors;

        }
      
        public async Task<ResponseDto> SaveCountries()
        {
            try
            {
                var response = await _client.GetAsync("https://restcountries.com/v2/all?fields=name,capital,region,population,flag,currencies");
                var responseRates = await _client.GetAsync("https://open.er-api.com/v6/latest/USD");
                JToken rates = new JObject();
                var isResponseFailed = !response.IsSuccessStatusCode || !responseRates.IsSuccessStatusCode; ;
                var errorsOfValidation = new Dictionary<string, string>();

                if (responseRates.IsSuccessStatusCode)
                {
                    var content = await responseRates.Content.ReadAsStringAsync();
                    var JSONObject = JObject.Parse(content);
                    rates = JSONObject["rates"];
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    List<Country> countriesFromAPI = new List<Country>();
                    //var countryJSON = JsonSerializer.Deserialize<List<dynamic>>(content);
                    JArray countryJSON = JArray.Parse(content);
                    for (int i = 0; i < countryJSON.Count; i++)
                    {
                        try
                        {
                            var countryInAPI = new Country();
                            var countryJsonObject = countryJSON[i];

                            countryInAPI.Name = (string)countryJsonObject["name"];
                            countryInAPI.Capital = (string?)countryJsonObject["capital"];
                            countryInAPI.Region = (string?)countryJsonObject["region"];
                            countryInAPI.Population = (int?)countryJsonObject["population"];
                            countryInAPI.Flag_url = (string?)countryJsonObject["Flag_url"];


                            if (countryJSON[i]["currencies"] == null || countryJSON[i]["currencies"].Count() == 0)
                            {
                                countryInAPI.Currency_code = null;
                                countryInAPI.Exchange_rate = null;
                                countryInAPI.Estimated_gdp = 0;
                            }
                            else
                            {
                                var currencyInfo = countryJSON[i]["currencies"][0];
                                countryInAPI.Currency_code = (string?)currencyInfo["code"];
                                countryInAPI.Exchange_rate = (int?)rates[(string)currencyInfo["code"]];
                            }
                            countriesFromAPI.Add(countryInAPI);


                        }
                        catch
                        {
                            countriesFromAPI[i].Currency_code = null;
                            countriesFromAPI[i].Exchange_rate = null;
                            countriesFromAPI[i].Estimated_gdp = 0;
                        }
                    }
                    var last_refresh = DateTime.UtcNow;
                    errorsOfValidation = IsValidationSuccess(countriesFromAPI);
                    if (errorsOfValidation.Count() == 0)
                    {
                        await SaveCountriesAfterCheckInDb(countriesFromAPI, last_refresh);
                        //await GenerateImage(countriesFromAPI, last_refresh);
                        return new ResponseDto
                        {
                            Error = "No"
                        };
                    }
                    else
                    {
                        return new ResponseDto
                        {
                            Error = "Validation failed",
                            DetailsValidation =  errorsOfValidation
                        };
                    }
                }
                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseDto
                    {
                        Error = "External data source unavailable",
                        Details =
                        "Could not fetch data from the countries API"
                    }
                    ;
                }
                else if (response.IsSuccessStatusCode)
                {
                    return new ResponseDto
                    {
                        Error = "External data source unavailable",
                        Details =
                        "Could not fetch data from the exchange rates API"
                    };
                }
                else
                {
                    new ResponseDto
                    {
                        Error = "No"
                    };


                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ResponseDto
                {
                    Error = "External data source unavailable",
                    Details =
     "Could not fetch data from the countries API"
                };
 ;
            }
            return new ResponseDto
            {
                Error = "External data source unavailable",
                Details =
 "Could not fetch data from the countries API"
            };

        }

        public async Task<List<Country>> GetCountriesFiltered(string? region = null,
            string? currency = null, string? sort = null)
        {
            var countries = await _context.Countries.OrderBy(p => p.Id).ToListAsync();
            var countriesFiltered = countries.Where(p => IsCountryConcerned(p, region, currency)).ToList();
            return countriesFiltered;

        }
        public bool IsCountryConcerned(Country country, string? region = null, string? currency = null)
        {
            var condition = true;
            if (region != null)
            {
                condition = condition && country.Region == region;
            }
            if (currency != null)
            {
                condition = condition && country.Currency_code == currency;
            }

            return condition;
        }
        public async Task<Country> GetCountry(string name)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(p => p.Name == name);
            return country;
        }
        public object PropertyConcerned(Country country, Dictionary<string, object> props_sort)
        {
            if(props_sort.ContainsKey("name"))
            {
                return country.Name;
            }
            else if(props_sort.ContainsKey("population"))
            {
                return country.Population;
            }
            else if(props_sort.ContainsKey("region"))
            {
                return country.Region;
            }
            else if(props_sort.ContainsKey("estimated_gdp"))
            {
                return country.Estimated_gdp;
            }
            else if (props_sort.ContainsKey("capital"))
            {
                return country.Capital;
            }
            else if (props_sort.ContainsKey("currency_code"))
            {
                return country.Currency_code;
            }
            else if(props_sort.ContainsKey("exchange_rate"))
            {
                return country.Exchange_rate;
            }
            else if(props_sort.ContainsKey("flag_url"))
            {
                return country.Flag_url;
            }
            else
            {
                return country.Id;
            }
        }
        public List<Country> GetCountriesSorted(List<Country> countries, Dictionary<string, object> props_sort)
        {
            var countriesSorted = new List<Country>();
            if(props_sort.Values.First().ToString() == "desc")
            {
                countriesSorted = countries.OrderByDescending(p =>
                {
                    return PropertyConcerned(p, props_sort);
                }).ToList();
                return countriesSorted;
            }
            else
            {
                countriesSorted = countries.OrderBy(p =>
                {
                    return PropertyConcerned(p, props_sort);
                }).ToList();
                return countriesSorted;
            }
               
        }

       

    }
}
