using System;
using System.Collections.Generic;
using AreaControl.Instances.Scenery;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.Model
{
    public class PoliceSlot : AbstractVehicleSlot
    {
        private readonly List<Barrier> _barriers = new List<Barrier>();
        private Ped _pedPreview;

        public PoliceSlot(Vector3 position, float laneHeading)
            : base(position, (laneHeading + 30f) % 360)
        {
            OriginalRoadHeading = laneHeading;
            PedHeading = RoadUtils.OppositeHeading(laneHeading);
            PedPosition = position + MathHelper.ConvertHeadingToDirection(PedHeading) * 4f;

            CreateBarriers();
        }

        #region Properties

        /// <summary>
        /// Get the heading of the block slot.
        /// </summary>
        public float OriginalRoadHeading { get; }

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

        #endregion

        #region IPreviewSupport

        public override void CreatePreview()
        {
            base.CreatePreview();
            _pedPreview = new Ped(ModelUtils.GetLocalCop(PedPosition), PedPosition, PedHeading);
            PreviewUtils.TransformToPreview(_pedPreview);
        }

        public override void DeletePreview()
        {
            base.DeletePreview();
            EntityUtils.Remove(_pedPreview);
            _pedPreview = null;
        }

        #endregion

        #region Functions

        protected override Rage.Model GetModelInstance()
        {
            return ModelUtils.GetLocalPolice(Position);
        }

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