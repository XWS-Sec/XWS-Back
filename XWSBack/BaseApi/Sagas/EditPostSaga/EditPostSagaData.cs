using System;
using NServiceBus;

namespace BaseApi.Sagas.EditPostSaga
{
    public class EditPostSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public Guid InternalPictureId { get; set; }
        public Guid PostId { get; set; }
    }
}