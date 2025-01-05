using PhonebookSoln.Core.Entities;
using PhonebookSoln.Core.Dtos;
using PhonebookSoln.Core.Interfaces;
using PhonebookSoln.Application.Interfaces;

namespace PhonebookSoln.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPhonebookRepository _repository;

        public PersonService(IPhonebookRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PersonDto>> GetAllPersonsAsync()
        {
            var persons = await _repository.GetAllPersonsAsync();
            var personDtos = new List<PersonDto>();

            foreach (var person in persons)
            {
                personDtos.Add(new PersonDto
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Company = person.Company,
                    Location = person.Location
                });
            }

            return personDtos;
        }

        public async Task<PersonDto> GetPersonByIdAsync(Guid personId)
        {
            var person = await _repository.GetPersonByIdAsync(personId);

            if (person == null) return null;

            return new PersonDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Company = person.Company,
                Location = person.Location
            };
        }

        public async Task AddPersonAsync(PersonDto personDto)
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = personDto.FirstName,
                LastName = personDto.LastName,
                Company = personDto.Company,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Location = personDto.Location,
                ContactInfos = personDto.ContactInfos?.Select(ci => new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    PersonId = Guid.NewGuid(),
                    InfoType = ci.InfoType,
                    InfoContent = ci.InfoContent
                }).ToList() ?? new List<ContactInfo>()
            };

            await _repository.AddPersonAsync(person);
        }

        public async Task UpdatePersonAsync(PersonDto personDto)
        {
            var person = await _repository.GetPersonByIdAsync(personDto.Id);

            if (person == null) return;

            person.FirstName = personDto.FirstName;
            person.LastName = personDto.LastName;
            person.Company = personDto.Company;
            person.UpdatedDate = DateTime.UtcNow;
            person.Location = personDto.Location;

            // Update ContactInfos
            foreach (var contactInfoDto in personDto.ContactInfos)
            {
                var existingContactInfo = person.ContactInfos
                    .FirstOrDefault(ci => ci.Id == contactInfoDto.Id);

                if (existingContactInfo != null)
                {
                    existingContactInfo.InfoContent = contactInfoDto.InfoContent;
                    existingContactInfo.InfoType = contactInfoDto.InfoType;
                }
                else
                {
                    person.ContactInfos.Add(new ContactInfo
                    {
                        Id = Guid.NewGuid(),
                        PersonId = person.Id,
                        InfoType = contactInfoDto.InfoType,
                        InfoContent = contactInfoDto.InfoContent
                    });
                }
            }

            await _repository.UpdatePersonAsync(person);
        }

        public async Task DeletePersonAsync(Guid personId)
        {
            await _repository.DeletePersonAsync(personId);
        }
    }
}