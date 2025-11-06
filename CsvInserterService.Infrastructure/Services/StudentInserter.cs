using CsvInserterService.Application.Interfaces;
using CsvInserterService.Infrastructure.Data;
using CsvInserterService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CsvInserterService.Infrastructure.Services
{
    public class StudentInserter : IStudentInserter
    {
        private readonly ApplicationDbContext _context;

        public StudentInserter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertStudentsFromCsvAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            Console.WriteLine($"📂 Reading CSV file: {filePath}");

            var students = new List<Student>();

            using var reader = new StreamReader(filePath);
            string? headerLine = await reader.ReadLineAsync(); // skip header

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');

                if (columns.Length < 3)
                    continue; // skip invalid lines

                var student = new Student
                {
                    Name = columns[0].Trim(),
                    Email = columns[1].Trim(),
                    Course = columns[2].Trim()
                };

                students.Add(student);
            }

            if (students.Count == 0)
            {
                Console.WriteLine("⚠️ No valid student rows found in CSV.");
                return;
            }

            await _context.Students.AddRangeAsync((IEnumerable<Domain.Entities.Student>)students);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Successfully inserted {students.Count} students into database.");
        }
    }
}
