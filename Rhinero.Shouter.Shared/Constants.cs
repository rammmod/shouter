namespace Rhinero.Shouter.Shared
{
    internal static class Constants
    {
        internal static class DefaultRabbitMQ
        {
            internal const string Exchange = "ShouterExchange";
            internal const string PublishQueue = "ShouterPublishQueue";
            internal const string ReplyQueue = "ShouterReplyQueue";
            internal const string VirtualHost = "/";
            internal const int Port = 5672;
            internal const int PrefetchCount = 16;
            internal const int ConcurrentMessageLimit = 16;
        }

        internal static class DefaultKafka
        {
            internal const string PublishTopic = "ShouterPublishTopic";
            internal const string PublishGroup = "ShouterGroup";
            internal const string RequestTopic = "ShouterRequestTopic";
            internal const string RequestGroup = "ShouterRequestGroup";
            internal const string ReplyTopic = "ShouterReplyTopic";
            internal const string ReplyGroup = "ShouterReplyGroup";
        }

        internal static class MassTransit
        {
            internal const string Queue = "queue:";
            internal const string Topic = "topic:";
        }

        internal static class StringCharacters
        {
            internal const string Ampersand = "&";
            internal const string EqualsSign = "=";
            internal const string QuestionMark = "?";
            internal const string Colon = ":";
            internal const string Dot = ".";
        }

        internal static class Http
        {
            internal const string BasicAuthentication = "Basic";
            internal const string BearerAuthentication = "Bearer";
            internal static string ContentType = "Content-Type";
        }

        internal static class Directories
        {
            internal const string ProtoFiles = "ProtoFiles";
            internal const string ProtoFilesGenerated = $"{ProtoFiles}\\Generated";
        }

        internal static class FileExtensions
        {
            internal const string WildcardCs = "*.cs";
            internal const string Cs = ".cs";

            internal const string WildcardProto = "*.proto";
            internal const string Proto = ".proto";

            internal const string GrpcName = "Grpc";
        }

        internal static class FileContentTypes
        {
            internal const string TextPlainContentType = "text/plain";
            internal const string OctetStreamContentType = "application/octet-stream";
        }

        internal static class Assembly
        {
            internal const string DynamicGrpcAssembly = "DynamicGrpcAssembly";
            internal const string Client = "Client";
            internal const string ClientBase = "ClientBase";
        }

        internal static class Task
        {
            internal const string Result = "Result";
            internal const string ResponseAsync = "ResponseAsync";
        }

        internal static class Redis
        {
            internal const double Lifetime = 60;
        }

        internal static class ReplyTimeout
        {
            internal const double LifetimeDouble = 30;
            internal const int LifetimeInt = 30;
        }

        internal static TimeSpan CancellationTokenTimeSpan = new TimeSpan(0, 0, 5);
    }
}
