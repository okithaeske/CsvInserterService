using System.Threading.Tasks;

namespace CsvInserterService.Application.Interfaces
{
    public interface IStudentInserter
    {
        Task InsertStudentsFromCsvAsync(string filePath);
    }
}
