namespace CsvInserterService.Domain.Entities
{
    public class CsvRow
    {
        public int Id { get; set; }
        public string RawData { get; set; } = string.Empty;
    }
}
