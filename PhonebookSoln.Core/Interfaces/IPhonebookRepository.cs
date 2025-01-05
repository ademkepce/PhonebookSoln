using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Entities;

namespace PhonebookSoln.Core.Interfaces
{
    public interface IPhonebookRepository
    {
        // Person işlemleri
        Task<Person> GetPersonByIdAsync(Guid personId);
        Task<IEnumerable<Person>> GetAllPersonsAsync();
        Task AddPersonAsync(Person person);
        Task UpdatePersonAsync(Person person);
        Task DeletePersonAsync(Guid personId);
        Task<int> GetPersonCountByLocationAsync(string location);
        Task<int> GetPhoneCountByLocationAsync(string location);

        // ContactInfo işlemleri
        Task<ContactInfo> GetContactInfoByIdAsync(Guid contactInfoId);
        Task<IEnumerable<ContactInfo>> GetContactInfosByPersonIdAsync(Guid personId);
        Task AddContactInfoAsync(ContactInfo contactInfo);
        Task UpdateContactInfoAsync(ContactInfo contactInfo);
        Task DeleteContactInfoAsync(Guid contactInfoId);

        // Report işlemleri
        Task<Report> GetReportByIdAsync(Guid reportId);
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task AddReportAsync(Report report);
        Task UpdateReportAsync(Report report);
        Task DeleteReportAsync(Guid reportId);

        // Outbox işlemleri
        Task AddOutboxMessageAsync(OutboxMessage message);
        Task MarkOutboxMessageAsProcessedAsync(Guid messageId);
        Task<IEnumerable<OutboxMessage>> GetUnprocessedOutboxMessagesAsync();
    }
}