using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2_HNG13.Data;
using Task2_HNG13.DTOs;

namespace Task2_HNG13.Controllers
{
    [Route("status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private AppDbContext _appDbContext;
        public StatusController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var countries = await _appDbContext.Countries.ToListAsync();
                var statusDto = new StatusDto
                {
                    Total_countries = countries.Count(),
                    Last_refreshed_at = countries[0].Last_refreshed_at

                };
                return Ok(statusDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        
    }
}