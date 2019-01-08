using Rage;

namespace AreaControl.Duties
{
    public interface IDutyManager
    {
        /// <summary>
        /// Get the next available duty in the area of the given position.
        /// </summary>
        /// <param name="position">Set the position to search in for an available duty.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty GetNextAvailableDuty(Vector3 position);
    }
}