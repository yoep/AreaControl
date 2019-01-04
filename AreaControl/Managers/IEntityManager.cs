using AreaControl.Instances;
using Rage;

namespace AreaControl.Managers
{
    public interface IEntityManager
    {
        /// <summary>
        /// Find a <see cref="Vehicle"/> within the given range of the position.
        /// If no <see cref="Vehicle"/> found within the range, create a new one.
        /// </summary>
        /// <param name="position">Set the position to search from.</param>
        /// <param name="radius">Set the range around the position to search within.</param>
        /// <returns>Returns a Area Controlled vehicle within the given range of the position.</returns>
        ACVehicle FindVehicleWithinOrCreate(Vector3 position, float radius);
    }
}