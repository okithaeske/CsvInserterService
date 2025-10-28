using CsvInserterService.Application.Interfaces;
using CsvInserterService.Domain.Entities;
using CsvInserterService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CsvInserterService.Infrastructure.Services;

public class StudentInserter : IStudentInserter
{
    private readonly ILogger<StudentInserter> _logger;
    private readonly IDatabase _redisDb;
    private readonly AppDbContext _db;

    public StudentInserter(
        ILogger<StudentInserter> logger,
        IConnectionMultiplexer redis,
        AppDbContext db)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
        _db = db;
    }

    [Obsolete]
    public async Task InsertAsync(Student student)
    {
        try
        {
            // Save to PostgreSQL
            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            // Notify via Redis
            var message = JsonSerializer.Serialize(new
            {
                name = student.Name,
                email = student.Email,
                course = student.Course,
                timestamp = DateTime.UtcNow
            });

            _ = await _redisDb.PublishAsync("csv-progress", message);

            _logger.LogInformation($"[DB INSERT] {student.Name} added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert student record");
        }
    }
}
    