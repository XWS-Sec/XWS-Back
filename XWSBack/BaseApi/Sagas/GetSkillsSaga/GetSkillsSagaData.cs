using System;
using NServiceBus;

namespace BaseApi.Sagas.GetSkillsSaga
{
    public class GetSkillsSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public string LinkName { get; set; }
    }
}