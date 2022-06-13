using System;
using System.Threading.Tasks;
using BaseApi.Controllers.Base;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Chats;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class ChatController : SyncController
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageSession _session;

        public ChatController(UserManager<User> userManager, IMessageSession session, IMemoryCache cache) : base(cache)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet("{page}/{otherUserId}")]
        public async Task<IActionResult> Get(int page, Guid otherUserId)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginGetChatRequest()
            {
                Page = page,
                CorrelationId = Guid.NewGuid(),
                UserId = userId,
                OtherUserId = otherUserId
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            
            return ReturnBaseNotification(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewMessageDto messageDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var request = new BeginAddMessageRequest()
            {
                Message = messageDto.Message,
                CorrelationId = Guid.NewGuid(),
                DateCreated = DateTime.Now,
                ReceiverId = messageDto.ReceiverId,
                SenderId = userId
            };
            await _session.SendLocal(request).ConfigureAwait(false);

            var response = SyncResponse(request.CorrelationId);
            
            return ReturnBaseNotification(response);
        }
    }
}