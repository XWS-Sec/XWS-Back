using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using Users.Graph.Handlers.Services;
using Users.Graph.Messages.Follow;

namespace Users.Graph.Handlers.Handlers.Follow
{
    public class RecommendedNewLinksHandler : IHandleMessages<RecommendNewLinksRequest>
    {
        private readonly UserNodeService _userNodeService;
        private readonly RecommenderService _recommenderService;

        public RecommendedNewLinksHandler(UserNodeService userNodeService, RecommenderService recommenderService)
        {
            _userNodeService = userNodeService;
            _recommenderService = recommenderService;
        }

        public async Task Handle(RecommendNewLinksRequest message, IMessageHandlerContext context)
        {
            var validationStatus = await ValidateMessage(message);

            if (!string.IsNullOrEmpty(validationStatus))
            {
                await context.Reply(new RecommendNewLinksResponse()
                {
                    CorrelationId = message.CorrelationId,
                    MessageToLog = validationStatus
                }).ConfigureAwait(false);
                return;
            }

            var links = await _recommenderService.GetPotentialLinks(message.UserId);
            links = links.Where(x => x != message.UserId).ToList();
            
            await context.Reply(new RecommendNewLinksResponse()
            {
                Recommended = links,
                CorrelationId = message.CorrelationId,
                IsSuccessful = true,
                MessageToLog = "Successful"
            }).ConfigureAwait(false);
        }

        private async Task<string> ValidateMessage(RecommendNewLinksRequest message)
        {
            var retVal = string.Empty;

            if (message.UserId == Guid.Empty)
            {
                retVal += "User is mandatory";
            }
            else if (!await _userNodeService.UserExists(message.UserId))
            {
                retVal += "User not found";
            }
            
            return retVal;
        }
    }
}