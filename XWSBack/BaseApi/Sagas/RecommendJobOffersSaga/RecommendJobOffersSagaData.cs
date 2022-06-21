using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseApi.Sagas.RecommendJobOffersSaga
{
    public class RecommendJobOffersSagaData : ContainSagaData
    {
        public Guid CorrelationId { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<string> Interests { get; set; }
    }
}
