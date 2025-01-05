using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Application.Interfaces
{
    public interface IPersonService
    {
        Task<IEnumerable<PersonDto>> GetAllPersonsAsync();
        Task<PersonDto> GetPersonByIdAsync(Guid personId);
        Task AddPersonAsync(PersonDto personDto);
        Task UpdatePersonAsync(PersonDto personDto);
        Task DeletePersonAsync(Guid personId);
    }
}