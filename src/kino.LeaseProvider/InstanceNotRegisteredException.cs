using System;

namespace kino.LeaseProvider
{
    public class InstanceNotRegisteredException : Exception
    {
        public InstanceNotRegisteredException()
        {
        }

        public InstanceNotRegisteredException(string message)
            : base(message)
        {
        }
    }
}