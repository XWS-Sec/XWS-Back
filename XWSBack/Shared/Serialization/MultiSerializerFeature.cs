using NServiceBus;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NServiceBus.Pipeline;

namespace Shared.Serialization
{
    public class MultiSerializerFeature : Feature
    {
        internal MultiSerializerFeature()
        {
            EnableByDefault();
        }
        
        protected override void Setup(FeatureConfigurationContext context)
        {
            PipelineSettings pipeline = context.Pipeline;
            pipeline.Replace("DeserializeLogicalMessagesConnector", typeof(DeserializeConnector));
            pipeline.Replace("SerializeMessageConnector", typeof(SerializeConnector));
            IConfigureComponents container = context.Container;
            container.ConfigureComponent<SerializationMapper>(DependencyLifecycle.SingleInstance);
        }
    }
}