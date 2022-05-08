using System;
using NServiceBus;

namespace BaseApi.Sagas.AdjustSkillsSaga
{
    public class AdjustSkillsSagaData : ContainSagaData
    {
        public Guid UserId { get; set; }
        public Guid CorrelationId { get; set; }
        public string LinkName { get; set; }
    }
}