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

        public Vector3 Position { get; }

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