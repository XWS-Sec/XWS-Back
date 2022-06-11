using System;

namespace BaseApi.Services.Exceptions
{
    public class EmailingException : Exception
    {
        public EmailingException(string message) : base(message)
        {
        }
    }
}