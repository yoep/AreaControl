using AreaControl.Instances;

namespace AreaControl.Duties
{
    public interface IDuty
    {
        /// <summary>
        /// Get if this duty is still active.
        /// Some duties can be completed and will become inactive when completed.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Execute this duty on the given ped.
        /// </summary>
        /// <param name="ped">Set the ped instance.</param>
        void Execute(ACPed ped);
    }
}