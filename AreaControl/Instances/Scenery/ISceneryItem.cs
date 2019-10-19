using AreaControl.Duties;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public interface ISceneryItem : IPreviewSupport
    {
        /// <summary>
        /// Get the position of the scenery item.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the scenery item.
        /// </summary>
        float Heading { get; }

        /// <summary>
        /// Get the place object instance for this scenery item.
        /// </summary>
        PlaceObjectsDuty.PlaceObject Object { get; }
    }
}