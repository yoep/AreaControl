using System;

namespace AreaControl.Duties.Exceptions
{
    public class DutyException : Exception
    {
        public DutyException(string message) 
            : base(message)
        {
        }

        public DutyException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}