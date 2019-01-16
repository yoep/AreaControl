using System;

namespace AreaControl.Duties
{
    public class InvalidDutyStateException : Exception
    {
        public InvalidDutyStateException(string message, DutyState state)
            : base(message + ",state: " + state)
        {
        }
    }
}