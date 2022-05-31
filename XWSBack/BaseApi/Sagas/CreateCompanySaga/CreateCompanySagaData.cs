using System;
using NServiceBus;

namespace BaseApi.Sagas.CreateCompanySaga
{
    public class CreateCompanySagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
    }
}