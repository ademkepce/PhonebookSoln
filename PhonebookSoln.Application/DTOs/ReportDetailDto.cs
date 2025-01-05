using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Application.DTOs
{
    public class ReportDetailDto : ReportDto
    {
        public int PersonCountInLocation { get; set; }
        public int PhoneCountInLocation { get; set; }
    }
}
