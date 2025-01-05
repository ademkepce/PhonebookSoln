using PhonebookSoln.Core.Entities;
using PhonebookSoln.Core.Dtos;
using PhonebookSoln.Core.Interfaces;
using PhonebookSoln.Application.Interfaces;

namespace PhonebookSoln.Application.Services
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly IPhonebookRepository _repository;

        public ContactInfoService(IPhonebookRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ContactInfoDto>> GetContactInfosByPersonIdAsync(Guid personId)
        {
            var contactInfos = await _repository.GetContactInfosByPersonIdAsync(personId);
            var contactInfoDtos = new List<ContactInfoDto>();

            foreach (var contactInfo in contactInfos)
            {
                contactInfoDtos.Add(new ContactInfoDto
                {
                    Id = contactInfo.Id,
                    PersonId = contactInfo.PersonId,
                    InfoType = contactInfo.InfoType,
                    InfoContent = contactInfo.InfoContent
                });
            }

            return contactInfoDtos;
        }

        public async Task<ContactInfoDto> GetContactInfoByIdAsync(Guid contactInfoId)
        {
            var contactInfo = await _repository.GetContactInfoByIdAsync(contactInfoId);

            if (contactInfo == null) return null;

            return new ContactInfoDto
            {
                Id = contactInfo.Id,
                PersonId = contactInfo.PersonId,
                InfoType = contactInfo.InfoType,
                InfoContent = contactInfo.InfoContent
            };
        }

        public async Task AddContactInfoAsync(ContactInfoDto contactInfoDto)
        {
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                PersonId = contactInfoDto.PersonId,
                InfoType = contactInfoDto.InfoType,
                InfoContent = contactInfoDto.InfoContent,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _repository.AddContactInfoAsync(contactInfo);
        }

        public async Task UpdateContactInfoAsync(ContactInfoDto contactInfoDto)
        {
            var contactInfo = await _repository.GetContactInfoByIdAsync(contactInfoDto.Id);

            if (contactInfo == null) return;

            contactInfo.InfoType = contactInfoDto.InfoType;
            contactInfo.InfoContent = contactInfoDto.InfoContent;
            contactInfo.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateContactInfoAsync(contactInfo);
        }

        public async Task DeleteContactInfoAsync(Guid contactInfoId)
        {
            await _repository.DeleteContactInfoAsync(contactInfoId);
        }
    }
}