using System;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines that an active duty is already present for the Ped.
    /// </summary>
    public class ActiveDutyException : Exception
    {
        public ActiveDutyException(string message)
            : base(message)
        {
        }
    }
}