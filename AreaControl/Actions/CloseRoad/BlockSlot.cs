using System;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Actions.CloseRoad
{
    public class BlockSlot
    {
        public BlockSlot(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        /// <summary>
        /// Get the position of the block slot.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the block slot.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Creates a preview of the expected vehicle block position.
        /// </summary>
        public void CreatePreview()
        {
            PreviewUtil.TransformToPreview(new Vehicle("POLICE", Position, Heading));
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}," + Environment.NewLine +
                   $"{nameof(Heading)}: {Heading}";
        }
    }
}