using System;
using AreaControl.Instances;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Actions.CloseRoad
{
    public class BlockSlot : IPreviewSupport
    {
        private Vehicle _vehiclePreview;
        private Ped _pedPreview;

        public BlockSlot(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading - 55f;
            PedHeading = heading + 90f;
            PedPosition = position + MathHelper.ConvertHeadingToDirection(PedHeading) * 4f;
        }

        /// <summary>
        /// Get the position of the block slot.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the position of the traffic ped.
        /// </summary>
        public Vector3 PedPosition { get; }

        /// <summary>
        /// Get the heading of the block slot.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the heading of the traffic ped.
        /// </summary>
        public float PedHeading { get; }

        /// <inheritdoc />
        public bool IsPreviewActive => _vehiclePreview != null;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _vehiclePreview = new Vehicle("POLICE", Position, Heading);
            _pedPreview = new Ped(new Model("s_m_y_cop_01"), PedPosition, PedHeading);
            PreviewUtil.TransformToPreview(_vehiclePreview);
            PreviewUtil.TransformToPreview(_pedPreview);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            _vehiclePreview.Delete();
            _vehiclePreview = null;
            _pedPreview.Delete();
            _pedPreview = null;
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}," + Environment.NewLine +
                   $"{nameof(Heading)}: {Heading}, " + Environment.NewLine +
                   $"{nameof(PedPosition)}: {PedPosition}, " + Environment.NewLine +
                   $"{nameof(PedHeading)}: {PedHeading}";
        }
    }
}