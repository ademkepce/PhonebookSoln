using Moq;
using PhonebookSoln.Application.Services;
using PhonebookSoln.Core.Dtos;
using PhonebookSoln.Core.Entities;
using PhonebookSoln.Core.Enums;
using PhonebookSoln.Core.Interfaces;

namespace PhonebookSoln.Tests
{
    public class PersonServiceTests
    {
        private readonly Mock<IPhonebookRepository> _mockRepository;
        private readonly PersonService _service;

        public PersonServiceTests()
        {
            _mockRepository = new Mock<IPhonebookRepository>();
            _service = new PersonService(_mockRepository.Object);
        }

        [Fact]
        public async Task AddPersonAsync_ShouldAddPerson()
        {
            var personDto = new PersonDto
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp",
                Location = "New York",
                ContactInfos = new List<ContactInfoDto>
                {
                    new ContactInfoDto { InfoType = ContactInfoType.Phone, InfoContent = "1234567890" }
                }
            };

            await _service.AddPersonAsync(personDto);

            _mockRepository.Verify(repo => repo.AddPersonAsync(It.IsAny<Person>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPersonsAsync_ShouldReturnPersons()
        {
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
            };

            _mockRepository.Setup(repo => repo.GetAllPersonsAsync()).ReturnsAsync(persons);

            var result = await _service.GetAllPersonsAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetPersonByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            var personId = Guid.NewGuid();
            var person = new Person { Id = personId, FirstName = "John", LastName = "Doe" };

            _mockRepository.Setup(repo => repo.GetPersonByIdAsync(personId)).ReturnsAsync(person);

            var result = await _service.GetPersonByIdAsync(personId);

            Assert.NotNull(result);
            Assert.Equal(personId, result.Id);
        }

        [Fact]
        public async Task GetPersonByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist()
        {
            var personId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.GetPersonByIdAsync(personId)).ReturnsAsync((Person)null);

            var result = await _service.GetPersonByIdAsync(personId);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePersonAsync_ShouldUpdatePerson()
        {
            var personDto = new PersonDto
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp",
                Location = "New York",
                ContactInfos = new List<ContactInfoDto>
                {
                    new ContactInfoDto { Id = Guid.NewGuid(), InfoType = ContactInfoType.Phone, InfoContent = "1234567890" }
                }
            };

            var existingPerson = new Person
            {
                Id = personDto.Id,
                FirstName = "OldFirstName",
                LastName = "OldLastName",
                Company = "OldCompany",
                Location = "OldLocation",
                ContactInfos = new List<ContactInfo>
                {
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = ContactInfoType.Phone, InfoContent = "0987654321" }
                }
            };

            _mockRepository.Setup(repo => repo.GetPersonByIdAsync(personDto.Id)).ReturnsAsync(existingPerson);

            await _service.UpdatePersonAsync(personDto);

            _mockRepository.Verify(repo => repo.UpdatePersonAsync(It.IsAny<Person>()), Times.Once);
            Assert.Equal(personDto.FirstName, existingPerson.FirstName);
        }

        [Fact]
        public async Task DeletePersonAsync_ShouldDeletePerson()
        {
            var personId = Guid.NewGuid();

            await _service.DeletePersonAsync(personId);

            _mockRepository.Verify(repo => repo.DeletePersonAsync(personId), Times.Once);
        }
    }
}