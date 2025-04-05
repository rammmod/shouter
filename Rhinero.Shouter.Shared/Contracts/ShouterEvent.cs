using Rhinero.Shouter.Client;
using MassTransit;
using System.Net;

namespace Rhinero.Shouter.Shared.Contracts
{
    public sealed class ShouterEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; } = Guid.NewGuid();
        public Uri Uri { get; init; }
        public ShouterMethodEnums Method { get; init; }
        public string Payload { get; init; }
        public string ContentType { get; init; }
        public string Token { get; init; }
        public NetworkCredential Credentials { get; init; }
    }
}
