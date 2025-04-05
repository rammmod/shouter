using Rhinero.Shouter.Client;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared.Contracts;
using Rhinero.Utils.Common.Http;

namespace Rhinero.Shouter.Services
{
    public class CallbackService : ICallbackService
    {
        public async Task Send(ShouterEvent message)
        {
            //AppHttpClient client;

            //if (message.Credentials is null && string.IsNullOrWhiteSpace(message.Token))
            //    client = new AppHttpClient("");
            //else if (!string.IsNullOrWhiteSpace(message.Token))
            //    client = new AppHttpClient("", token: message.Token);
            //else if (message.Credentials is not null && !string.IsNullOrEmpty(message.Credentials.UserName) && !string.IsNullOrEmpty(message.Credentials.Password))
            //    client = new AppHttpClient("", credentials: message.Credentials);
            //else
            //    throw new NotImplementedException();


            //switch (message.Method)
            //{
            //    case MethodEnums.Post:
            //        await client.PostAsync(message.Uri.AbsoluteUri, message.Payload);
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}


            AppHttpClient2 client2;

            if (message.Credentials is null && string.IsNullOrWhiteSpace(message.Token))
                client2 = new AppHttpClient2("", message.ContentType);
            else if (!string.IsNullOrWhiteSpace(message.Token))
                client2 = new AppHttpClient2("", token: message.Token, message.ContentType);
            else if (message.Credentials is not null && !string.IsNullOrEmpty(message.Credentials.UserName) && !string.IsNullOrEmpty(message.Credentials.Password))
                client2 = new AppHttpClient2("", credentials: message.Credentials, message.ContentType);
            else
                throw new NotImplementedException();


            switch (message.Method)
            {
                case ShouterMethodEnums.Post:
                    await client2.PostAsync(message.Uri.AbsoluteUri, message.Payload);
                    break;
                case ShouterMethodEnums.Get:
                    await client2.GetTaskAsync(message.Uri.AbsoluteUri + message.Payload, 100);
                    break;
                case ShouterMethodEnums.Put:
                    throw new NotImplementedException();
                case ShouterMethodEnums.Patch:
                    throw new NotImplementedException();
                case ShouterMethodEnums.Delete:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
