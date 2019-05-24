using System;
using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Instances.Scenery;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.CloseRoad
{
    public class BlockSlot : IPreviewSupport
    {
        private readonly List<Barrier> _barriers = new List<Barrier>();
        private readonly List<Entity> _previewObjects = new List<Entity>();

        public BlockSlot(Vector3 position, float laneHeading)
        {
            OriginalRoadHeading = laneHeading;
            Position = position;
            Heading = (laneHeading + 30f) % 360;
            PedHeading = RoadUtils.OppositeHeading(laneHeading);
            PedPosition = position + MathHelper.ConvertHeadingToDirection(PedHeading) * 4f;

            CreateBarriers();
        }

        /// <summary>
        /// Get the heading of the block slot.
        /// </summary>
        public float OriginalRoadHeading { get; }

        /// <summary>
        /// Get the position of the block slot.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the block slot.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the position of the traffic ped.
        /// </summary>
        public Vector3 PedPosition { get; }

        /// <summary>
        /// Get the heading of the traffic ped.
        /// </summary>
        public float PedHeading { get; }

        /// <summary>
        /// Get the barriers for this road block slot.
        /// </summary>
        public IReadOnlyList<Barrier> Barriers => _barriers.AsReadOnly();

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObjects.Count > 0;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _previewObjects.Add(new Vehicle("POLICE", Position, Heading));
            _previewObjects.Add(new Ped(new Model("s_m_y_cop_01"), PedPosition, PedHeading));
            _barriers.ForEach(x => _previewObjects.Add(PropUtils.CreatePoliceDoNotCrossBarrier(x.Position, x.Heading)));

            _previewObjects.ForEach(PreviewUtils.TransformToPreview);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            _previewObjects.ForEach(EntityUtils.Remove);
            _previewObjects.Clear();
        }

        #endregion

        #region Methods

        public void ClearSlotFromTraffic()
        {
            EntityUtils.CleanArea(Position, 5f);
        }

        #endregion

        #region Functions

        private void CreateBarriers()
        {
            var barrierHeading = RoadUtils.OppositeHeading(OriginalRoadHeading);
            var position = PedPosition + MathHelper.ConvertHeadingToDirection(barrierHeading) * 1f;
            var moveDirection = MathHelper.ConvertHeadingToDirection((barrierHeading + 90f) % 360);

            _barriers.Add(new Barrier(position + moveDirection * 1.5f, barrierHeading));
            _barriers.Add(new Barrier(position - moveDirection * 1.5f, barrierHeading));
        }

        #endregion

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}," + Environment.NewLine +
                   $"{nameof(Heading)}: {Heading}, " + Environment.NewLine +
                   $"{nameof(PedPosition)}: {PedPosition}, " + Environment.NewLine +
                   $"{nameof(PedHeading)}: {PedHeading}";
        }
    }
}