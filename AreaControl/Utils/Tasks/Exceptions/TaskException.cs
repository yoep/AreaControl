using System;

namespace AreaControl.Utils.Tasks.Exceptions
{
    public class TaskException : Exception
    {
        public TaskException(string message) : base(message)
        {
        }
    }
}