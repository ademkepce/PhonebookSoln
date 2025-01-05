using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Application.Interfaces
{
    public interface IContactInfoService
    {
        Task<IEnumerable<ContactInfoDto>> GetContactInfosByPersonIdAsync(Guid personId);
        Task<ContactInfoDto> GetContactInfoByIdAsync(Guid contactInfoId);
        Task AddContactInfoAsync(ContactInfoDto contactInfoDto);
        Task UpdateContactInfoAsync(ContactInfoDto contactInfoDto);
        Task DeleteContactInfoAsync(Guid contactInfoId);
    }
}