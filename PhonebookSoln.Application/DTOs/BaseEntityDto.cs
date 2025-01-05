namespace PhonebookSoln.Application.DTOs
{
    public abstract class BaseEntityDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
