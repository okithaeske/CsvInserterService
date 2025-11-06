using CsvInserterService.Domain.Entities;
using CsvInserterService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CsvInserterService.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CsvRow> CsvRows { get; set; }
        public DbSet<Domain.Entities.Student> Students => Set<Domain.Entities.Student>();
    }
}
