namespace AreaControl.Duties.Exceptions
{
    public class InvalidDutyStateException : DutyException
    {
        public InvalidDutyStateException(string message, DutyState state)
            : base(message + ",state: " + state)
        {
        }
    }
}