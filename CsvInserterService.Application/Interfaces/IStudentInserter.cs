using CsvInserterService.Domain.Entities;

namespace CsvInserterService.Application.Interfaces;

public interface IStudentInserter
{
    Task InsertAsync(Student student);
}
