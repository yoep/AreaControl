using System;

namespace AreaControl.Duties
{
    public class InvalidDutyStateException : Exception
    {
        public InvalidDutyStateException(DutyState state)
            : base("Duty is in an invalid state, state: " + state)
        {
        }
    }
}