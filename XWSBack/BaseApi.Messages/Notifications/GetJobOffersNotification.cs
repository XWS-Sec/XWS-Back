using System;
using System.Collections.Generic;
using BaseApi.Messages.Dtos;

namespace BaseApi.Messages.Notifications
{
    public class GetJobOffersNotification
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<JobOfferNotificationDto> JobOffers { get; set; }
    }
}