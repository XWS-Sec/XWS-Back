using System.Net;

namespace BaseApi.Messages.Notifications
{
    public class BaseNotification
    {
        public string JsonResponse { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}