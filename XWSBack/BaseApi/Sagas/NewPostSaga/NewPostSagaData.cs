using System;
using NServiceBus;

namespace BaseApi.Sagas.NewPostSaga
{
    public class NewPostSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public bool HasPicture { get; set; }
    }
}