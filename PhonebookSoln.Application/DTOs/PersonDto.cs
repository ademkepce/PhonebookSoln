using PhonebookSoln.Application.DTOs;

namespace PhonebookSoln.Core.Dtos
{
    public class PersonDto : BaseEntityDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public ICollection<ContactInfoDto> ContactInfos { get; set; }
    }
}