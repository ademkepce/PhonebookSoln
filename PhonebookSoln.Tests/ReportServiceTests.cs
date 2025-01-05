using Moq;
using PhonebookSoln.Application.Services;
using PhonebookSoln.Core.Dtos;
using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Entities;
using PhonebookSoln.Core.Enums;
using PhonebookSoln.Core.Interfaces;

namespace PhonebookSoln.Tests
{

    public class ReportServiceTests
    {
        private readonly Mock<IPhonebookRepository> _mockRepository;
        private readonly Mock<IRabbitMqService> _mockRabbitMqService;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _mockRepository = new Mock<IPhonebookRepository>();
            _mockRabbitMqService = new Mock<IRabbitMqService>();
            _service = new ReportService(_mockRepository.Object, _mockRabbitMqService.Object);
        }

        [Fact]
        public async Task GenerateReportAsync_ShouldUpdateReportStatusAndSendMessage()
        {
            var reportId = Guid.NewGuid();
            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.Preparing,
                Location = "New York",
                PersonCountInLocation = 10,
                PhoneCountInLocation = 20,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _mockRepository.Setup(repo => repo.GetReportByIdAsync(reportId)).ReturnsAsync(report);
            _mockRepository.Setup(repo => repo.UpdateReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
            _mockRepository.Setup(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);
            _mockRabbitMqService.Setup(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

            await _service.GenerateReportAsync(reportId);

            _mockRepository.Verify(repo => repo.UpdateReportAsync(It.IsAny<Report>()), Times.Exactly(2));
            _mockRepository.Verify(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
            _mockRabbitMqService.Verify(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
        }

        [Fact]
        public async Task GetAllReportsAsync_ShouldReturnReportDtos()
        {
            var reports = new List<Report>
        {
            new Report
            {
                Id = Guid.NewGuid(),
                Status = ReportStatus.Completed,
                Location = "New York",
                PersonCountInLocation = 10,
                PhoneCountInLocation = 20,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            }
        };

            _mockRepository.Setup(repo => repo.GetAllReportsAsync()).ReturnsAsync(reports);

            var result = await _service.GetAllReportsAsync();

            Assert.Single(result);
            Assert.Equal(reports[0].Id, result.First().Id);
            Assert.Equal(reports[0].Location, result.First().Location);
            Assert.Equal(reports[0].Status, result.First().Status);
            Assert.Equal((double)reports[0].PhoneCountInLocation / reports[0].PersonCountInLocation, result.First().AveragePhonePerPerson);
        }

        [Fact]
        public async Task GetReportByIdAsync_ShouldReturnReportDto()
        {
            var reportId = Guid.NewGuid();
            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.Completed,
                Location = "New York",
                PersonCountInLocation = 10,
                PhoneCountInLocation = 20,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _mockRepository.Setup(repo => repo.GetReportByIdAsync(reportId)).ReturnsAsync(report);

            var result = await _service.GetReportByIdAsync(reportId);

            Assert.Equal(report.Id, result.Id);
            Assert.Equal(report.Location, result.Location);
            Assert.Equal(report.Status, result.Status);
            Assert.Equal((double)report.PhoneCountInLocation / report.PersonCountInLocation, result.AveragePhonePerPerson);
        }

        [Fact]
        public async Task AddReportAsync_ShouldAddReportAndSendMessage()
        {
            var reportDto = new ReportDto
            {
                Location = "New York"
            };

            _mockRepository.Setup(repo => repo.AddReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
            _mockRepository.Setup(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);
            _mockRabbitMqService.Setup(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

            await _service.AddReportAsync(reportDto);

            _mockRepository.Verify(repo => repo.AddReportAsync(It.IsAny<Report>()), Times.Once);
            _mockRepository.Verify(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
            _mockRabbitMqService.Verify(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReportAsync_ShouldUpdateReportAndSendMessage()
        {
            var reportId = Guid.NewGuid();
            var reportDto = new ReportDto
            {
                Id = reportId,
                Location = "Los Angeles",
                Status = ReportStatus.Completed,
                PersonCount = 5,
                PhoneCount = 10
            };

            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.Preparing,
                Location = "New York",
                PersonCountInLocation = 0,
                PhoneCountInLocation = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _mockRepository.Setup(repo => repo.GetReportByIdAsync(reportId)).ReturnsAsync(report);
            _mockRepository.Setup(repo => repo.UpdateReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
            _mockRepository.Setup(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);
            _mockRabbitMqService.Setup(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

            await _service.UpdateReportAsync(reportDto);

            _mockRepository.Verify(repo => repo.UpdateReportAsync(It.IsAny<Report>()), Times.Once);
            _mockRepository.Verify(repo => repo.AddOutboxMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
            _mockRabbitMqService.Verify(rabbitMq => rabbitMq.SendMessageAsync(It.IsAny<OutboxMessage>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReportAsync_ShouldDeleteReport()
        {
            var reportId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.DeleteReportAsync(reportId)).Returns(Task.CompletedTask);

            await _service.DeleteReportAsync(reportId);

            _mockRepository.Verify(repo => repo.DeleteReportAsync(reportId), Times.Once);
        }
    }
}