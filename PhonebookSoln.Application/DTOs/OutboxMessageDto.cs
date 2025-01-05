namespace PhonebookSoln.Application.DTOs
{
    public class OutboxMessageDto : BaseEntityDto
    {
        public string MessageType { get; set; }
        public string Payload { get; set; }
        public bool Processed { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
