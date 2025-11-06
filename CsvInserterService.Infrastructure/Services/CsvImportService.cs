using CsvInserterService.Application.Interfaces;
using CsvInserterService.Domain.Entities;
using CsvInserterService.Infrastructure.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace CsvInserterService.Infrastructure.Services
{
    public class CsvImportService : ICsvImportService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CsvImportService> _logger;

        public CsvImportService(ApplicationDbContext db, ILogger<CsvImportService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ImportCsvAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true
            };

            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? Array.Empty<string>();

            var studentHeaders = new[] { "StudentId", "Name", "Email", "CourseName" };
            bool isStudentCsv = studentHeaders.All(h => headers.Contains(h, StringComparer.OrdinalIgnoreCase));

            int count = 0;

            if (isStudentCsv)
            {
                var records = csv.GetRecords<Student>().ToList();
                _db.Students.AddRange(records);
                count = records.Count;
                _logger.LogInformation($"? Detected Student CSV - inserted {count} student records.");
            }
            else
            {
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();
                using var csvGeneric = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                var records = csvGeneric.GetRecords<dynamic>();

                foreach (var record in records)
                {
                    var json = JsonSerializer.Serialize(record);
                    _db.CsvRows.Add(new CsvRow { RawData = json });
                    count++;
                }

                _logger.LogWarning($"?? CSV schema didn't match Student model. Inserted {count} rows into CsvRows as raw JSON.");
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation($"?? Saved {count} records from file: {filePath}");
        }

        public async Task<List<CsvRow>> GetAllRowsAsync()
        {
            return await _db.CsvRows.AsNoTracking().ToListAsync();
        }
    }
}
