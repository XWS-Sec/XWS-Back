using System;

namespace BaseApi.Messages.Dtos
{
    public class UserNotificationDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsPrivate { get; set; }
    }
}