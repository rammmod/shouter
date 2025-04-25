using MassTransit;

namespace Rhinero.Shouter.Contracts
{
    internal record ShouterReplyMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public string Payload { get; init; }
    }
}
