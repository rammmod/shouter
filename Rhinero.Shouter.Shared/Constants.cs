namespace Rhinero.Shouter.Shared
{
    internal static class Constants
    {
        internal static class DefaultRabbitMQ
        {
            internal const string Exchange = "ShouterExchange";
            internal const string Queue = "ShouterQueue";
            internal const string VirtualHost = "/";
            internal const int Port = 5672;
            internal const int PrefetchCount = 16;
            internal const int ConcurrentMessageLimit = 16;
        }

        internal static class DefaultKafka
        {
            internal const string Topic = "ShouterTopic";
            internal const string Group = "ShouterGroup";
        }

        internal static class StringCharacters
        {
            internal const string Ampersand = "&";
            internal const string EqualsSign = "=";
            internal const string QuestionMark = "?";
            internal const string Colon = ":";
        }


        internal static class Http
        {
            internal const string BasicAuthentication = "Basic";
            internal const string BearerAuthentication = "Bearer";
            internal static string ContentType = "Content-Type";
        }

        internal static TimeSpan CancellationTokenTimeSpan = new TimeSpan(0, 0, 5);
    }
}
