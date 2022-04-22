using System;
using Newtonsoft.Json;
using NServiceBus;
using Shared.Custom;

namespace Users.Graph.Messages
{
    public class CreateNodeRequest : ICustomCommand
    {
        public Guid NewUserId { get; set; }
    }
}