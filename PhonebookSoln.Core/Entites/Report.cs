using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Enums;

namespace PhonebookSoln.Core.Entities
{
    public class Report : BaseEntity
    {
        public ReportStatus Status { get; set; }
        public string Location { get; set; }
        public int PersonCountInLocation { get; set; }
        public int PhoneCountInLocation { get; set; }
    }
}