using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Contracts.Payloads;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Contracts.Payloads.Http;
using Rhinero.Shouter.Shared.Exceptions;

namespace Rhinero.Shouter.Contracts
{
    internal static class ShouterInterfaceChecker
    {
        internal static void CheckShouterMessageInterface(object payload)
        {
            if (payload is not IShouterPayload)
                throw new ShouterInterfaceImplementationException();
        }

        internal static void CheckProtocolAndMessage(Protocol protocol, object payload)
        {
            if (protocol is Protocol.HTTP && payload.GetType() != typeof(HttpPayload) ||
                protocol is Protocol.gRPC && payload.GetType() != typeof(GrpcPayload))
                throw new ProtocolAndMessageTypeInconsistencyException();
        }
    }
}
