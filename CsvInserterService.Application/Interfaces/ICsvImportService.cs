using CsvInserterService.Domain.Entities;

namespace CsvInserterService.Application.Interfaces;

public interface ICsvImportService
{
 Task ImportCsvAsync(string filePath);
 Task<List<CsvRow>> GetAllRowsAsync();
}
