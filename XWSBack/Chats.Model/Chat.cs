using System;
using System.Collections.Generic;

namespace Chats.Model
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IList<Guid> Members { get; set; }
        public IList<Message> Messages { get; set; }
    }
}