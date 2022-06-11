using System;
using System.Runtime.Serialization;

namespace BaseApi.Services.Exceptions
{
    [Serializable]
    public class EmailingException : Exception
    {
        public EmailingException(string message) : base(message)
        {
        }

        protected EmailingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}