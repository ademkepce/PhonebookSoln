using PhonebookSoln.Core.Entites;
using PhonebookSoln.Core.Enums;

namespace PhonebookSoln.Core.Entities
{
    public class ContactInfo : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public Person Person { get; set; }
        public ContactInfoType InfoType { get; set; }
        public string InfoContent { get; set; }
    }
}