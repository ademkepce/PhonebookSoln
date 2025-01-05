using Microsoft.EntityFrameworkCore;
using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Entities;

namespace PhonebookSoln.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<ContactInfo>().ToTable("ContactInfo");
            modelBuilder.Entity<Report>().ToTable("Report");
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage");

            modelBuilder.Entity<Person>().HasKey(p => p.Id);
            modelBuilder.Entity<ContactInfo>().HasKey(c => c.Id);
            modelBuilder.Entity<Report>().HasKey(r => r.Id);
            modelBuilder.Entity<OutboxMessage>().HasKey(r => r.Id);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var entity = modelBuilder.Entity(entityType.ClrType);
                entity.Property("CreatedDate").IsRequired();
                entity.Property("UpdatedDate").IsRequired();
            }

            modelBuilder.Entity<ContactInfo>()
                .HasOne(c => c.Person)
                .WithMany(p => p.ContactInfos)
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactInfo>(entity =>
            {
                entity.Property(c => c.InfoType)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(c => c.InfoContent)
                      .HasMaxLength(500)
                      .IsRequired();
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(r => r.Location)
                      .HasMaxLength(200);

                entity.Property(r => r.PersonCountInLocation)
                      .IsRequired();

                entity.Property(r => r.PhoneCountInLocation)
                      .IsRequired();
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(p => p.FirstName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(p => p.LastName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(p => p.Company)
                      .HasMaxLength(200);
            });
        }
    }
}