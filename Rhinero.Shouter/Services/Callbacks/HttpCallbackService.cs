using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Payloads.Http;
using Rhinero.Shouter.Helpers;
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

        public async Task SendAsync(ShouterMessage message, CancellationToken cancellationToken)
        {
            var payload = message.Payload.FromJson<HttpPayload>();
            var uriBuilder = new StringBuilder(payload.Uri.AbsoluteUri);

            HttpHelper.EnrichWithQueryParameters(uriBuilder, payload);

            var request = new HttpRequestMessage
            {
                Method = payload.Method,
                RequestUri = new Uri(uriBuilder.ToString())
            };

            HttpHelper.EnrichWithContent(request, payload);

            HttpHelper.EnrichWithHeaders(request, payload);

            HttpHelper.EnrichWithCredentials(request, payload);

            HttpHelper.EnrichWithToken(request, payload);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
