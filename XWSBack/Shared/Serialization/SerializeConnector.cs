using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Serialization;
using NServiceBus.Unicast.Messages;

namespace Shared.Serialization
{
    public class SerializeConnector : StageConnector<IOutgoingLogicalMessageContext, IOutgoingPhysicalMessageContext>
    {
        readonly SerializationMapper serializationMapper;
        readonly MessageMetadataRegistry messageMetadataRegistry;

        public SerializeConnector(SerializationMapper serializationMapper, MessageMetadataRegistry messageMetadataRegistry)
        {
            this.serializationMapper = serializationMapper;
            this.messageMetadataRegistry = messageMetadataRegistry;
        }

        public override async Task Invoke(IOutgoingLogicalMessageContext context, Func<IOutgoingPhysicalMessageContext, Task> stage)
        {
            if (context.ShouldSkipSerialization())
            {
                IOutgoingPhysicalMessageContext emptyMessageContext = this.CreateOutgoingPhysicalMessageContext(
                    new byte[0],
                    context.RoutingStrategies,
                    context);

                await stage(emptyMessageContext)
                    .ConfigureAwait(false);
                return;
            }

            Type messageType = context.Message.MessageType;
            IMessageSerializer messageSerializer = serializationMapper.GetSerializer(messageType);

            Dictionary<string, string> headers = context.Headers;
            headers[Headers.ContentType] = messageSerializer.ContentType;
            headers[Headers.EnclosedMessageTypes] = SerializeEnclosedMessageTypes(messageType);

            byte[] array = Serialize(messageSerializer, context);
            IOutgoingPhysicalMessageContext physicalMessageContext = this.CreateOutgoingPhysicalMessageContext(
                array,
                context.RoutingStrategies,
                context);

            await stage(physicalMessageContext)
                .ConfigureAwait(false);
        }

        private byte[] Serialize(IMessageSerializer messageSerializer, IOutgoingLogicalMessageContext context)
        {
            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(context.Message.Instance, stream);
                return stream.ToArray();
            }
        }

        private string SerializeEnclosedMessageTypes(Type messageType)
        {
            MessageMetadata metadata = messageMetadataRegistry.GetMessageMetadata(messageType);
            IEnumerable<Type> distinctTypes = metadata.MessageHierarchy.Distinct();
            return string.Join(";", distinctTypes.Select(t => t.AssemblyQualifiedName));
        }
    }
}
    