using System;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Dto.Chats;
using BaseApi.Messages;
using BaseApi.Model.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(CustomAuthorizeAttribute))]
    public class ChatController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMessageSession _session;

        public ChatController(UserManager<User> userManager, IMessageSession session)
        {
            _userManager = userManager;
            _session = session;
        }

        [HttpGet("{page}/{otherUserId}")]
        public async Task<IActionResult> Get(int page, Guid otherUserId)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _session.SendLocal(new BeginGetChatRequest()
            {
                Page = page,
                CorrelationId = Guid.NewGuid(),
                UserId = userId,
                OtherUserId = otherUserId
            }).ConfigureAwait(false);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewMessageDto messageDto)
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _session.SendLocal(new BeginAddMessageRequest()
            {
                Message = messageDto.Message,
                CorrelationId = Guid.NewGuid(),
                DateCreated = DateTime.Now,
                ReceiverId = messageDto.ReceiverId,
                SenderId = userId
            }).ConfigureAwait(false);
            
            return Ok();
        }
    }
}