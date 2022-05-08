using System;
using NServiceBus;

namespace BaseApi.Sagas.AddMessageSaga
{
    public class AddMessageSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public int RequestsSent { get; set; }
        public string ErrorDesc { get; set; }
    }
}