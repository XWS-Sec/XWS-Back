using System;
using System.Runtime.Serialization;

namespace BaseApi.Services.Exceptions
{
    [Serializable]
    public class FacebookException : Exception
    {
        public FacebookException(string message) : base(message)
        {
        }

        protected FacebookException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}