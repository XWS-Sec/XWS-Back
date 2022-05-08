using System;
using System.Collections.Generic;
using NServiceBus;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace Shared.Serialization
{
    public class SerializationMapper
    {
        private readonly IMessageSerializer jsonSerializer;
        private readonly IMessageSerializer xmlSerializer;

        public SerializationMapper(IMessageMapper mapper, ReadOnlySettings settings)
        {
            xmlSerializer = new XmlSerializer()
                .Configure(settings)(mapper);

            jsonSerializer = new NewtonsoftSerializer()
                .Configure(settings)(mapper);
        }

        public IMessageSerializer GetSerializer(Dictionary<string, string> headers)
        {
            if (!headers.TryGetValue(Headers.ContentType, out string contentType))
            {
                // default to Json
                return jsonSerializer;
            }
            if (contentType == jsonSerializer.ContentType)
            {
                return jsonSerializer;
            }
            if (contentType == xmlSerializer.ContentType)
            {
                return xmlSerializer;
            }
            throw new Exception($"Could not derive serializer for contentType='{contentType}'");
        }

        public IMessageSerializer GetSerializer(Type messageType)
        {
            return jsonSerializer;
        }
    }
}