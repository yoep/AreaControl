using Rage;

namespace AreaControl.Actions
{
    /// <summary>
    /// Object item that is part of an action's scenery.
    /// </summary>
    public class SceneryItem
    {
        public SceneryItem(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        /// <summary>
        /// Get the position of the cone.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the cone placement.
        /// </summary>
        public float Heading { get; }
    }
}