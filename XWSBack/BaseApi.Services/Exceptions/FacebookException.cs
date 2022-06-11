using System;

namespace BaseApi.Services.Exceptions
{
    public class FacebookException : Exception
    {
        public FacebookException(string message) : base(message)
        {
        }
    }
}