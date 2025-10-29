using Microsoft.EntityFrameworkCore;
using Task2_HNG13.Models;

namespace Task2_HNG13.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Country> Countries { get; set; }
    }
}
