using System;
using System.Collections.Generic;

namespace BaseApi.Dto.Following
{
    public class FollowInfoDto
    {
        public IEnumerable<Guid> Followers { get; set; }
        public IEnumerable<Guid> FollowerRequests { get; set; }
        public IEnumerable<Guid> Following { get; set; }
    }
}