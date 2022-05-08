using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages;
using Users.Graph.Model;

namespace Users.Graph.Handlers.Handlers
{
    public class CreateNodeRequestHandler : IHandleMessages<CreateNodeRequest>
    {
        private readonly IGraphClient _client;
        private readonly UserNodeService _userNodeService;

        public CreateNodeRequestHandler(IGraphClient client, UserNodeService userNodeService)
        {
            _client = client;
            _userNodeService = userNodeService;
        }
        
        public async Task Handle(CreateNodeRequest message, IMessageHandlerContext context)
        {
            if (await _userNodeService.UserExists(message.NewUserId))
                return;

            await _client.Cypher.Create("(u:UserNode $param)")
                .WithParam("param", new UserNode()
                {
                    UserId = message.NewUserId
                }).ExecuteWithoutResultsAsync();
        }
    }
}