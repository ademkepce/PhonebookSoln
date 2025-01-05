using Microsoft.EntityFrameworkCore;
using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Entities;
using PhonebookSoln.Core.Enums;
using PhonebookSoln.Core.Interfaces;
using PhonebookSoln.Infrastructure.Data;

namespace PhonebookSoln.Infrastructure.Repositories
{
    public class PhonebookRepository : IPhonebookRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IRabbitMqService _rabbitMqService;

        public PhonebookRepository(ApplicationDbContext context, IRabbitMqService rabbitMqService)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
        }

        // Person işlemleri
        public async Task<Person> GetPersonByIdAsync(Guid personId)
        {
            return await _context.Persons.Include(p => p.ContactInfos).FirstOrDefaultAsync(p => p.Id == personId);
        }

        public async Task<IEnumerable<Person>> GetAllPersonsAsync()
        {
            return await _context.Persons.Include(p => p.ContactInfos).ToListAsync();
        }

        public async Task AddPersonAsync(Person person)
        {
            if (person.ContactInfos != null && person.ContactInfos.Any())
            {
                foreach (var contactInfo in person.ContactInfos)
                {
                    contactInfo.Id = Guid.NewGuid();
                    contactInfo.PersonId = person.Id;
                }
            }

            await _context.Persons.AddAsync(person);
            await _context.ContactInfos.AddRangeAsync(person.ContactInfos);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePersonAsync(Person person)
        {
            _context.Persons.Update(person);
            await _context.SaveChangesAsync();

            foreach (var contactInfo in person.ContactInfos)
            {
                if (contactInfo.Id == Guid.Empty)
                {
                    await _context.ContactInfos.AddAsync(contactInfo);
                }
                else
                {
                    _context.ContactInfos.Update(contactInfo);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeletePersonAsync(Guid personId)
        {
            var person = await _context.Persons.Include(p => p.ContactInfos).FirstOrDefaultAsync(p => p.Id == personId);
            if (person != null)
            {
                _context.ContactInfos.RemoveRange(person.ContactInfos);
                await _context.SaveChangesAsync();

                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetPersonCountByLocationAsync(string location)
        {
            return await _context.Persons.CountAsync(p => p.Location == location);
        }

        public async Task<int> GetPhoneCountByLocationAsync(string location)
        {
            return await _context.ContactInfos
                .Where(c => c.InfoType == ContactInfoType.Phone && c.Person.Location == location)
                .CountAsync();
        }

        // ContactInfo işlemleri
        public async Task<ContactInfo> GetContactInfoByIdAsync(Guid contactInfoId)
        {
            return await _context.ContactInfos.FindAsync(contactInfoId);
        }

        public async Task<IEnumerable<ContactInfo>> GetContactInfosByPersonIdAsync(Guid personId)
        {
            return await _context.ContactInfos.Where(c => c.PersonId == personId).ToListAsync();
        }

        public async Task AddContactInfoAsync(ContactInfo contactInfo)
        {
            await _context.ContactInfos.AddAsync(contactInfo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContactInfoAsync(ContactInfo contactInfo)
        {
            _context.ContactInfos.Update(contactInfo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContactInfoAsync(Guid contactInfoId)
        {
            var contactInfo = await _context.ContactInfos.FindAsync(contactInfoId);
            if (contactInfo != null)
            {
                _context.ContactInfos.Remove(contactInfo);
                await _context.SaveChangesAsync();
            }
        }

        // Report işlemleri
        public async Task<Report> GetReportByIdAsync(Guid reportId)
        {
            return await _context.Reports.FindAsync(reportId);
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _context.Reports.ToListAsync();
        }

        public async Task AddReportAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(Report report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(Guid reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        // Outbox işlemleri
        public async Task AddOutboxMessageAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            await _rabbitMqService.SendMessageAsync(message);
        }

        public async Task MarkOutboxMessageAsProcessedAsync(Guid messageId)
        {
            var message = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message != null)
            {
                message.Processed = true;
                message.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedOutboxMessagesAsync()
        {
            return await _context.OutboxMessages.Where(m => !m.Processed).ToListAsync();
        }
    }
}