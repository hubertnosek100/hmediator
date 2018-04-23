using System;

namespace HMediator
{
    public class MediatorException : Exception
    {
        public MediatorException(string message) : base(message)
        {
        }

        public MediatorException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}