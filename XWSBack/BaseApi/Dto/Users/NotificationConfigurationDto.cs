namespace BaseApi.Dto.Users
{
    public class NotificationConfigurationDto
    {
        public bool NewPost { get; set; }
        public bool NewFollower { get; set; }
        public bool NewMessage { get; set; }
    }
}