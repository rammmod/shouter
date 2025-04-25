using Grpc.Core;
using MassTransit;
using Rhinero.Shouter.Contracts.Payloads.Grpc;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Exceptions.DynamicCompilation;
using Rhinero.Shouter.Shared.Extensions;
using System.Reflection;

namespace Rhinero.Shouter.Helpers
{
    internal static class GrpcHelper
    {
        internal static (Type clientType, Type requestType, Type replyType) GetMessageTypes(IProtoCache protoCache, GrpcPayload payload)
        {
            Assembly clientAssembly;
            Type clientType;

            if (!string.IsNullOrWhiteSpace(payload.Service.FileName) &&
                !string.IsNullOrWhiteSpace(payload.Service.ClientName))
            {
                clientAssembly = protoCache.GetAssemblyByKey(payload.Service.FileName);

                clientType = clientAssembly.GetTypes().FirstOrDefault(t =>
                    t.Name.Equals(payload.Service.ClientName + Constants.Assembly.Client) is true &&
                    t.BaseType?.Name.Equals(payload.Service.ClientName + Constants.Assembly.ClientBase) is true) ??
                    throw new ClientTypeInAssemblyNotFoundException(payload.Service.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(payload.Service.FileName))
            {
                clientAssembly = protoCache.GetAssemblyByKey(payload.Service.FileName);

                clientType = clientAssembly.GetTypes().FirstOrDefault(t =>
                    t.Name.EndsWith(Constants.Assembly.Client) &&
                    t.BaseType?.Name.Contains(Constants.Assembly.ClientBase) is true) ??
                    throw new ClientTypeInAssemblyNotFoundException(payload.Service.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(payload.Service.ClientName))
            {
                clientAssembly = protoCache.GetAssemblyByClient(payload.Service.ClientName);

                clientType = clientAssembly.GetTypes().FirstOrDefault(t =>
                    t.Name.Equals(payload.Service.ClientName + Constants.Assembly.Client) is true &&
                    t.BaseType?.Name.Equals(payload.Service.ClientName + Constants.Assembly.ClientBase) is true) ??
                    throw new ClientTypeInAssemblyNotFoundException(payload.Service.FileName);
            }
            else
                throw new FileNameOrClientNameObligatoryException();

            var requestType = clientAssembly.GetTypes().First(t => t.Name.EndsWith(payload.RequestArgumentName));
            var replyType = clientAssembly.GetTypes().First(t => t.Name.EndsWith(payload.ResponseArgumentName));

            return (clientType, requestType, replyType);
        }

        internal static void EnrichWithRequestValues(object target, Dictionary<string, string> values)
        {
            foreach (var kvp in values)
            {
                var path = kvp.Key.Split(Constants.StringCharacters.Dot);
                object currentObj = target;
                Type currentType = target.GetType();

                for (int i = 0; i < path.Length; i++)
                {
                    var prop = currentType.GetProperty(path[i]);

                    if (prop is null)
                        return;

                    if (i == path.Length - 1)
                    {
                        object convertedValue = Convert.ChangeType(kvp.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        prop.SetValue(currentObj, convertedValue);
                    }
                    else
                    {
                        var nextObj = prop.GetValue(currentObj);

                        if (nextObj is null)
                        {
                            nextObj = Activator.CreateInstance(prop.PropertyType);
                            prop.SetValue(currentObj, nextObj);
                        }

                        currentObj = nextObj;
                        currentType = prop.PropertyType;
                    }
                }
            }
        }

        internal static MethodInfo GetRequestMethod(Type clientType, Type requestType, GrpcPayload payload)
        {
            Type[] requestTypes = [
                requestType,
                typeof(Grpc.Core.Metadata),
                typeof(DateTime?),
                typeof(CancellationToken)
            ];

            return clientType.GetMethod(payload.RequestMethod, requestTypes) ??
                throw new RequestMethodNotFoundInAssemblyException(payload.RequestMethod);
        }

        internal static Metadata GetMetadata(GrpcPayload payload)
        {
            var metadata = new Metadata();

            if (!payload.RequestMetadata.IsNullOrEmpty())
            {
                foreach (var item in payload.RequestMetadata)
                    metadata.Add(item.Key, item.Value);
            }

            return metadata;
        }

        internal static async Task<object> GetResponseFromInvokationAsync(object result)
        {
            object response = null;
            var resultType = result?.GetType();

            if (typeof(Task).IsAssignableFrom(resultType)) // Handle Task<T> or AsyncUnaryCall<T>.ResponseAsync
            {
                if (resultType.IsGenericType) // Case: Task<T>
                {
                    var task = (Task)result;
                    await task.ConfigureAwait(false);
                    response = resultType.GetProperty(Constants.Task.Result)?.GetValue(task);
                }
                else // Case: Task (non-generic), just await
                {
                    await(Task)result;
                    response = null;
                }
            }
            else if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Grpc.Core.AsyncUnaryCall<>)) // Handle AsyncUnaryCall<T>
            {
                var responseAsyncProp = resultType.GetProperty(Constants.Task.ResponseAsync);
                var responseTask = (Task)responseAsyncProp?.GetValue(result);

                if (responseTask is not null)
                {
                    await responseTask.ConfigureAwait(false);
                    response = responseTask.GetType().GetProperty(Constants.Task.Result)?.GetValue(responseTask);
                }
            }
            else // Synchronous return (direct reply object)
            {
                response = result;
            }

            return response;
        }
    }
}
