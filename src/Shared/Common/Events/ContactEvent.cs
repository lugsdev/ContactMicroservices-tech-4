namespace Common.Events
{
    public abstract class ContactEvent
    {
        public int ContactId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public abstract string EventType { get; }
    }

    public class ContactCreatedEvent : ContactEvent
    {
        public override string EventType => "ContactCreated";
        public string Nome { get; set; } = string.Empty;
        public string DDD { get; set; } = string.Empty;
        public string NumeroCelular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ContactUpdatedEvent : ContactEvent
    {
        public override string EventType => "ContactUpdated";
        public string Nome { get; set; } = string.Empty;
        public string DDD { get; set; } = string.Empty;
        public string NumeroCelular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? DataAtualizacao { get; set; }
    }

    public class ContactDeletedEvent : ContactEvent
    {
        public override string EventType => "ContactDeleted";
        public string Nome { get; set; } = string.Empty;
    }
}

