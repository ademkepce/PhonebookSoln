using PhonebookSoln.Core.Dtos;
using PhonebookSoln.Core.Interfaces;
using PhonebookSoln.Core.Entites;
using Newtonsoft.Json;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Core.Enums;
using PhonebookSoln.Core.Entities;

namespace PhonebookSoln.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IPhonebookRepository _repository;
        private readonly IRabbitMqService _rabbitMqService;

        public ReportService(IPhonebookRepository repository, IRabbitMqService rabbitMqService)
        {
            _repository = repository;
            _rabbitMqService = rabbitMqService;
        }

        public async Task GenerateReportAsync(Guid reportId)
        {
            var report = await _repository.GetReportByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found.");

            report.Status = ReportStatus.Preparing;
            report.UpdatedDate = DateTime.UtcNow;
            await _repository.UpdateReportAsync(report);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ReportGenerated",
                Payload = JsonConvert.SerializeObject(new { ReportId = reportId }),
                CreatedDate = DateTime.UtcNow,
                Processed = false
            };

            await _repository.AddOutboxMessageAsync(outboxMessage);
            await _rabbitMqService.SendMessageAsync(outboxMessage);

            await Task.Delay(5000);  // Örnek olarak 5 saniye bekletelim (rapor oluşturma süresi)

            report.Status = ReportStatus.Completed;
            report.UpdatedDate = DateTime.UtcNow;
            await _repository.UpdateReportAsync(report);
        }

        public async Task<IEnumerable<ReportDto>> GetAllReportsAsync()
        {
            var reports = await _repository.GetAllReportsAsync();
            var reportDtos = new List<ReportDto>();

            foreach (var report in reports)
            {
                var averagePhonePerPerson = report.PersonCountInLocation > 0
                    ? (double)report.PhoneCountInLocation / report.PersonCountInLocation
                    : 0;

                reportDtos.Add(new ReportDto
                {
                    Id = report.Id,
                    Status = report.Status,
                    Location = report.Location,
                    PersonCount = report.PersonCountInLocation,
                    PhoneCount = report.PhoneCountInLocation,
                    AveragePhonePerPerson = averagePhonePerPerson,
                    CreatedDate = report.CreatedDate,
                    UpdatedDate = report.UpdatedDate
                });
            }

            return reportDtos;
        }

        public async Task<ReportDto> GetReportByIdAsync(Guid reportId)
        {
            var report = await _repository.GetReportByIdAsync(reportId);

            if (report == null) return null;

            var averagePhonePerPerson = report.PersonCountInLocation > 0
                ? (double)report.PhoneCountInLocation / report.PersonCountInLocation
                : 0;

            return new ReportDto
            {
                Id = report.Id,
                Status = report.Status,
                Location = report.Location,
                PersonCount = report.PersonCountInLocation,
                PhoneCount = report.PhoneCountInLocation,
                AveragePhonePerPerson = averagePhonePerPerson,
                CreatedDate = report.CreatedDate,
                UpdatedDate = report.UpdatedDate
            };
        }

        public async Task AddReportAsync(ReportDto reportDto)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                Status = ReportStatus.Preparing,
                Location = reportDto.Location,
                PersonCountInLocation = 0,
                PhoneCountInLocation = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _repository.AddReportAsync(report);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "NewReportAdded",
                Payload = JsonConvert.SerializeObject(report),
                CreatedDate = DateTime.UtcNow,
                Processed = false
            };

            await _repository.AddOutboxMessageAsync(outboxMessage);

            // Kuyruğa mesaj gönderilir
            await _rabbitMqService.SendMessageAsync(outboxMessage);
        }

        public async Task UpdateReportAsync(ReportDto reportDto)
        {
            var report = await _repository.GetReportByIdAsync(reportDto.Id);
            if (report == null) return;

            report.Location = reportDto.Location;
            report.Status = reportDto.Status;
            report.PersonCountInLocation = reportDto.PersonCount;
            report.PhoneCountInLocation = reportDto.PhoneCount;
            report.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateReportAsync(report);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ReportUpdated",
                Payload = JsonConvert.SerializeObject(report),
                CreatedDate = DateTime.UtcNow,
                Processed = false
            };

            await _repository.AddOutboxMessageAsync(outboxMessage);

            // Kuyruğa mesaj gönderilir
            await _rabbitMqService.SendMessageAsync(outboxMessage);
        }

        public async Task DeleteReportAsync(Guid reportId)
        {
            await _repository.DeleteReportAsync(reportId);
        }
    }
}