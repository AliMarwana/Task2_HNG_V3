using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Task2_HNG13.Data;
using Task2_HNG13.Filters;
using Task2_HNG13.Models;
using Task2_HNG13.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task2_HNG13.Controllers
{
    [Route("countries")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private CountryRepository _countryRepository;
        private SqlQueryGenerator _sqlQueryGenerator;
        private IWebHostEnvironment _environment;
        private AppDbContext _context;
        private readonly string cacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
        public CountriesController(CountryRepository countryRepository, SqlQueryGenerator sqlQueryGenerator,
            IWebHostEnvironment environment, AppDbContext context)
        {
            _countryRepository = countryRepository;
            _sqlQueryGenerator = sqlQueryGenerator;
            _environment = environment;
            _context = context;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCountries()
        {
            try
            {
                var responseDto = await _countryRepository.SaveCountries();
                if (responseDto.Error != "No")
                {
                    if (responseDto.Error == "Validation failed")
                    {
                        var error = new
                        {
                            Error = responseDto.Error,
                            Details = responseDto.DetailsValidation
                        };
                        return BadRequest(error);
                    }
                    else
                    {
                        var error = new
                        {
                            Error = responseDto.Error,
                            Details = responseDto.Details
                        };
                        return StatusCode(503, error);
                    }


                }
                else
                {

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }

            //return Ok(countries);
        }

        [HttpGet]
        public async Task<IActionResult> GetCountriesFiltered([FromQuery]string? region = null,
           [FromQuery] string? currency = null, [FromQuery] string? sort = null)
        {
            try
            {
                List<Country> countries = await _countryRepository.GetCountriesFiltered(region, currency, sort);
                if (sort == null)
                {
                    return Ok(countries);
                }
                else
                {
                    var sorting_properties = _sqlQueryGenerator.GenerateSqlQueryAsync(sort ?? "").Result;
                    var countries_sorted = _countryRepository.GetCountriesSorted(countries, sorting_properties);
                    return Ok(countries_sorted);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }

        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetCountriesByName(string name)
        {
            try
            {
                var country = await _countryRepository.GetCountry(name);
                if (country == null)
                {
                    return NotFound("Country not found");
                }
                else
                {
                    return Ok(country);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteCountry(string name)
        {
            try
            {
                var country = await _countryRepository.GetCountry(name);
                if (country == null)
                {
                    return NotFound("Country not found");
                }
                else
                {
                    _context.Countries.Remove(country);
                    await _context.SaveChangesAsync();  
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetImage()
        {
            try
            {
                string rootPath = Path.Combine(_environment.ContentRootPath, "cache");
                var imageName = "countries_report.png";
                var imagesDirectory = rootPath;

                // Ensure the directory exists
                if (!Directory.Exists(imagesDirectory))
                {
                    return NotFound($"Directory '{imagesDirectory}' not found");
                }

                // Construct full file path
                string filePath = Path.Combine(imagesDirectory, imageName);

                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = "Summary image not found" });
                }

                // Determine content type based on file extension
                string contentType = GetContentType(imageName);

                // Return the file
                return PhysicalFile(filePath, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }

        }
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }


    }
}
