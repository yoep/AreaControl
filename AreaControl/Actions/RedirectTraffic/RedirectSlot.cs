using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectSlot : IPreviewSupport
    {
        private const float PedDistanceFromVehicle = 3f;
        private readonly List<Entity> _previewObjects = new List<Entity>();
        private readonly List<Cone> _cones = new List<Cone>();

        public RedirectSlot(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
            PedHeading = RoadUtil.OppositeHeading(Heading);
            PedPosition = Position + MathHelper.ConvertHeadingToDirection(PedHeading) * PedDistanceFromVehicle;
            CreateCones();
        }

        /// <summary>
        /// Get the position of the redirect slot.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the redirect slot.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get the position of the ped for the redirect slot.
        /// </summary>
        public Vector3 PedPosition { get; }

        /// <summary>
        /// Get the heading of the ped for the redirect slot.
        /// </summary>
        public float PedHeading { get; }

        /// <summary>
        /// Get the cones to place for this redirect traffic slot.
        /// </summary>
        public IReadOnlyList<Cone> Cones => _cones.AsReadOnly();

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObjects.Count > 0;

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            _previewObjects.Add(new Vehicle("POLICE", Position, Heading));
            _previewObjects.Add(new Ped(new Model("s_m_y_cop_01"), PedPosition, PedHeading));
            _previewObjects.ForEach(PreviewUtil.TransformToPreview);
            _cones.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            _previewObjects.ForEach(EntityUtil.Remove);
            _previewObjects.Clear();
            _cones.ForEach(x => x.DeletePreview());
        }

        #endregion

        public override string ToString()
        {
            return
                $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(PedPosition)}: {PedPosition}, {nameof(PedHeading)}: {PedHeading}, " +
                $"{nameof(IsPreviewActive)}: {IsPreviewActive}";
        }

        private void CreateCones()
        {
            var directionLeftOfVehicle = MathHelper.ConvertHeadingToDirection(Heading + 90f);
            var moveHeading = PedHeading;
            var directionBehindVehicle = MathHelper.ConvertHeadingToDirection(moveHeading);
            var positionLeftOfVehicle = Position + directionLeftOfVehicle * 2f;
            var secondCone = positionLeftOfVehicle + directionBehindVehicle * 2f;
            var thirdCone = secondCone + MathHelper.ConvertHeadingToDirection(moveHeading + 40f) * 1.5f;

            _cones.Add(new Cone(positionLeftOfVehicle, moveHeading));
            _cones.Add(new Cone(secondCone, moveHeading));
            _cones.Add(new Cone(thirdCone, moveHeading));
        }

        public class Cone : IPreviewSupport
        {
            private Object _previewObject;

            public Cone(Vector3 position, float heading)
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

            #region IPreviewSupport implementation

            /// <inheritdoc />
            public bool IsPreviewActive => _previewObject != null;

            /// <inheritdoc />
            public void CreatePreview()
            {
                if (IsPreviewActive)
                    return;

                _previewObject = PropUtil.CreateSmallConeWithStripes(Position);
                PreviewUtil.TransformToPreview(_previewObject);
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                PropUtil.Remove(_previewObject);
                _previewObject = null;
            }

            #endregion
        }
    }
}