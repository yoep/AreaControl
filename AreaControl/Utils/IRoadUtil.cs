using AreaControl.Model;
using Rage;

namespace AreaControl.Utils
{
    public interface IRoadUtil
    {
        /// <summary>
        /// Get the location of the closest road in regards to the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        Road GetClosestRoad(Vector3 position, RoadType roadType);
    }

    /// <summary>
    /// Defines the road types on which can be searched.
    /// </summary>
    public enum RoadType
    {
        All,
        MajorRoadsOnly
    }
}