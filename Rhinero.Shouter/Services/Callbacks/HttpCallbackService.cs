using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Payloads.Http;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Extensions;
using System.Net.Http.Headers;
using System.Text;

namespace Rhinero.Shouter.Services.Callbacks
{
    public class HttpCallbackService : ICallbackService
    {
        private readonly HttpClient _httpClient;

        public HttpCallbackService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendAsync(ShouterMessage message)
        {
            var payload = message.Payload.FromJson<HttpPayload>();
            var uriBuilder = new StringBuilder(payload.Uri.AbsoluteUri);

            if (payload.QueryParameters.NotNullOrEmpty())
            {
                uriBuilder.Append(Constants.StringCharacters.QuestionMark);
                uriBuilder.Append(string.Join(Constants.StringCharacters.Ampersand, payload.QueryParameters.Select(q =>
                    string.Concat(Uri.EscapeDataString(q.Key), Constants.StringCharacters.EqualsSign, Uri.EscapeDataString(q.Value)))));
            }

            var request = new HttpRequestMessage
            {
                Method = payload.Method,
                RequestUri = new Uri(uriBuilder.ToString())
            };

            if (!string.IsNullOrWhiteSpace(payload.Body) &&
                (payload.Method == HttpMethod.Post ||
                 payload.Method == HttpMethod.Put ||
                 payload.Method == HttpMethod.Patch))
                request.Content = new StringContent(payload.Body, Encoding.UTF8, GetContentType(payload.ContentType));

            if (payload.Headers.NotNullOrEmpty())
            {
                foreach (var header in payload.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (payload.Credentials is not null)
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(Constants.Http.BasicAuthentication,
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(
                        string.Concat(payload.Credentials.UserName, Constants.StringCharacters.Colon, payload.Credentials.Password))));

            if (!string.IsNullOrWhiteSpace(payload.Token) && request.Headers.Authorization is null)
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(Constants.Http.BearerAuthentication, payload.Token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private static string GetContentType(ContentTypeEnum contentType)
        {
            return contentType switch
            {
                ContentTypeEnum.Json => "application/json",
                ContentTypeEnum.Xml => "application/xml",
                _ => throw new NotImplementedException()
            };
        }
    }
}
