using Rhinero.Shouter.Contracts.Enums;
using Rhinero.Shouter.Contracts.Payloads;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Contracts.Payloads.Http;
using Rhinero.Shouter.Shared.Exceptions;
using System.Text.Json;
using System.Xml;

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

        internal static void CheckForHttpPayload(Protocol protocol, object payload)
        {
            if (protocol is Protocol.HTTP && payload is IHttpPayload)
            {
                var httpPayload = payload as IHttpPayload;

                try
                {
                    switch (httpPayload.ContentType)
                    {
                        case ContentTypeEnum.Json:
                            {
                                JsonDocument.Parse(httpPayload.Body);
                            }
                            break;
                        case ContentTypeEnum.Xml:
                            {
                                new XmlDocument().Load(httpPayload.Body);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch
                {
                    throw new HttpBodyAndContentTypeInconsistencyException();
                }
            }
        }
    }
}
