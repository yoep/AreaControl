using System;

namespace AreaControl.Duties
{
    public class DutyAvailableEventArgs : EventArgs
    {
        public DutyAvailableEventArgs(IDuty availableDuty)
        {
            AvailableDuty = availableDuty;
        }

        /// <summary>
        /// Get the available duty.
        /// </summary>
        public IDuty AvailableDuty { get; }
    }
}