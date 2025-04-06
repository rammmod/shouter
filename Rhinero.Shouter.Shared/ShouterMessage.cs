using MassTransit;
using Rhinero.Shouter.Shared;

namespace Rhinero.Shouter.Contracts
{
    public record ShouterMessage : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; } = Guid.NewGuid();
        internal ProtocolEnum Protocol { get; init; }
        internal string Payload { get; init; }
    }
}
