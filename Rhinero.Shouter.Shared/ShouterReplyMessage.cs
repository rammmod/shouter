using MassTransit;

namespace Rhinero.Shouter.Contracts
{
    public record ShouterReplyMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string Payload { get; init; }
        public string Error { get; init; }
    }
}
