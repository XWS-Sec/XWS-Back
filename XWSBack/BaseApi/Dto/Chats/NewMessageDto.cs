using System;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Dto.Chats
{
    public class NewMessageDto
    {
        [Required(ErrorMessage = "Receiver is mandatory")]
        public Guid ReceiverId { get; set; }
        [Required(ErrorMessage = "Message is mandatory")]
        public string Message { get; set; }
    }
}