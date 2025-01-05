using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Application.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetAllReportsAsync();
        Task<ReportDto> GetReportByIdAsync(Guid reportId);
        Task GenerateReportAsync(Guid reportId);
        Task AddReportAsync(ReportDto reportDto);
        Task UpdateReportAsync(ReportDto reportDto);
        Task DeleteReportAsync(Guid reportId);
    }
}