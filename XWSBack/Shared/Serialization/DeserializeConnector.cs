using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Serialization;
using NServiceBus.Transport;
using NServiceBus.Unicast.Messages;

namespace Shared.Serialization
{
    public class DeserializeConnector : StageConnector<IIncomingPhysicalMessageContext, IIncomingLogicalMessageContext>
    {
        readonly SerializationMapper serializationMapper;
        readonly MessageMetadataRegistry messageMetadataRegistry;
        readonly LogicalMessageFactory logicalMessageFactory;
        static readonly ILog log = LogManager.GetLogger<DeserializeConnector>();

        public DeserializeConnector(
            SerializationMapper serializationMapper,
            MessageMetadataRegistry messageMetadataRegistry,
            LogicalMessageFactory logicalMessageFactory)
        {
            this.serializationMapper = serializationMapper;
            this.messageMetadataRegistry = messageMetadataRegistry;
            this.logicalMessageFactory = logicalMessageFactory;
        }

        public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<IIncomingLogicalMessageContext, Task> stage)
        {
            IncomingMessage incomingMessage = context.Message;

            List<LogicalMessage> messages = ExtractWithExceptionHandling(incomingMessage);

            foreach (LogicalMessage message in messages)
            {
                IIncomingLogicalMessageContext logicalMessageContext = this.CreateIncomingLogicalMessageContext(message, context);
                await stage(logicalMessageContext).ConfigureAwait(false);
            }
        }

        private List<LogicalMessage> ExtractWithExceptionHandling(IncomingMessage message)
        {
            try
            {
                return Extract(message);
            }
            catch (Exception exception)
            {
                throw new MessageDeserializationException(message.MessageId, exception);
            }
        }

        private List<LogicalMessage> Extract(IncomingMessage physicalMessage)
        {
            if (physicalMessage.Body == null || physicalMessage.Body.Length == 0)
            {
                return new List<LogicalMessage>();
            }

            var messageMetadata = new List<MessageMetadata>();

            Dictionary<string, string> headers = physicalMessage.Headers;
            if (headers.TryGetValue(Headers.EnclosedMessageTypes, out string messageTypeIdentifier))
            {
                foreach (string messageTypeString in messageTypeIdentifier.Split(';'))
                {
                    string typeString = messageTypeString;
                    MessageMetadata metadata = messageMetadataRegistry.GetMessageMetadata(typeString);
                    if (metadata == null)
                    {
                        continue;
                    }

                    messageMetadata.Add(metadata);
                }

                if (
                    messageMetadata.Count == 0 &&
                    physicalMessage.GetMessageIntent() != MessageIntentEnum.Publish)
                {
                    log.Warn($"Could not determine message type from message header '{messageTypeIdentifier}'. MessageId: {physicalMessage.MessageId}");
                }
            }

            using (var stream = new MemoryStream(physicalMessage.Body))
            {
                IMessageSerializer messageSerializer = serializationMapper.GetSerializer(headers);
                List<Type> typesToDeserialize = messageMetadata
                    .Select(metadata => metadata.MessageType)
                    .ToList();
                return messageSerializer.Deserialize(stream, typesToDeserialize)
                    .Select(x => logicalMessageFactory.Create(x.GetType(), x))
                    .ToList();
            }
        }
    }
}