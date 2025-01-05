using PhonebookSoln.Core.Entites;

namespace PhonebookSoln.Core.Entities
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public ICollection<ContactInfo> ContactInfos { get; set; }
    }
}