using CsvInserterService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CsvInserterService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; }
}
