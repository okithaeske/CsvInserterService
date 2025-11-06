using CsvInserterService.Application.Interfaces;
using CsvInserterService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CsvInserterService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvRowsController : ControllerBase
{
 private readonly ICsvImportService _csvImportService;

 public CsvRowsController(ICsvImportService csvImportService)
 {
 _csvImportService = csvImportService;
 }

 [HttpPost("upload")]
 public async Task<IActionResult> UploadCsv([FromForm] IFormFile file)
 {
 if (file == null || file.Length ==0)
 return BadRequest("No file uploaded.");

 var filePath = Path.GetTempFileName();
 using (var stream = System.IO.File.Create(filePath))
 {
 await file.CopyToAsync(stream);
 }

 await _csvImportService.ImportCsvAsync(filePath);
 return Ok("CSV imported successfully.");
 }

 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var rows = await _csvImportService.GetAllRowsAsync();
 return Ok(rows);
 }
}
