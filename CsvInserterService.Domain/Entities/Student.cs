using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CsvInserterService.Domain.Entities
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ auto-generate ID
        public int Id { get; set; }

        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CourseName { get; set; }
    }
}
