using System;
using System.Runtime.Serialization;

namespace Orangebeard.SpecFlowPlugin.LogHandler
{
    public class InvalidContextException : Exception
    {
        public InvalidContextException()
        {
        }
        public InvalidContextException(string message) : base(message)
        {
        }
        public InvalidContextException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public InvalidContextException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}