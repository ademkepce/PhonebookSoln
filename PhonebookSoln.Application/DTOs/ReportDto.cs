using PhonebookSoln.Application.DTOs;
using PhonebookSoln.Core.Enums;

namespace PhonebookSoln.Core.Dtos
{
    public class ReportDto : BaseEntityDto
    {
        public ReportStatus Status { get; set; }
        public string Location { get; set; }
        public int PersonCount { get; set; }
        public int PhoneCount { get; set; }
        public double AveragePhonePerPerson { get; set; }
    }
}