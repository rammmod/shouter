using MassTransit;
using Rhinero.Shouter.Shared;

namespace Rhinero.Shouter.Contracts
{
    public record ShouterMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public ProtocolEnum Protocol { get; init; }
        public string Payload { get; init; }
    }
}
