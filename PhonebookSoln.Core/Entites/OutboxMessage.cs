namespace PhonebookSoln.Core.Entites
{
    public class OutboxMessage : BaseEntity
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; }
        public string Payload { get; set; }
        public bool Processed { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
