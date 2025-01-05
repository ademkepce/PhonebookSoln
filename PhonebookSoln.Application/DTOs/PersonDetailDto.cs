using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Application.DTOs
{
    public class PersonDetailDto : PersonDto
    {
        public List<ContactInfoDto> ContactInfos { get; set; }
    }
}
