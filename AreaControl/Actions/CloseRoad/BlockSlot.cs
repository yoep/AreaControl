using System;
using Rage;
using Rage.Native;

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
            var previewVehicle = new Vehicle("POLICE", Position, Heading)
            {
                Opacity = 0.7f,
                NeedsCollision = false,
                IsPositionFrozen = true,
                IsDriveable = false
            };
            NativeFunction.Natives.SET_ENTITY_COLLISION(previewVehicle, false, false);
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}," + Environment.NewLine +
                   $"{nameof(Heading)}: {Heading}";
        }
    }
}