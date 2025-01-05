using Microsoft.AspNetCore.Mvc;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return Ok(reports);
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(Guid reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);

            if (report == null)
            {
                return NotFound($"Report with ID {reportId} not found.");
            }

            return Ok(report);
        }

        [HttpPost]
        public async Task<IActionResult> AddReport([FromBody] ReportDto reportDto)
        {
            await _reportService.AddReportAsync(reportDto);
            return CreatedAtAction(nameof(GetReportById), new { reportId = reportDto.Id }, reportDto);
        }

        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(Guid reportId, [FromBody] ReportDto reportDto)
        {
            if (reportId != reportDto.Id)
            {
                return BadRequest("Report ID mismatch.");
            }

            await _reportService.UpdateReportAsync(reportDto);
            return NoContent();
        }

        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(Guid reportId)
        {
            await _reportService.DeleteReportAsync(reportId);
            return NoContent();
        }

        [HttpPost("generate/{reportId}")]
        public async Task<IActionResult> GenerateReport(Guid reportId)
        {
            try
            {
                await _reportService.GenerateReportAsync(reportId);
                return Ok("Rapor oluşturuluyor.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}