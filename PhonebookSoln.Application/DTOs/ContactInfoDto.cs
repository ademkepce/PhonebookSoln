using PhonebookSoln.Application.DTOs;
using PhonebookSoln.Core.Enums;

namespace PhonebookSoln.Core.Dtos
{
    public class ContactInfoDto : BaseEntityDto
    {
        public Guid PersonId { get; set; }
        public ContactInfoType InfoType { get; set; }
        public string InfoContent { get; set; }
    }
}