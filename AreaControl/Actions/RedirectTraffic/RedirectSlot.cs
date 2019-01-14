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
            var lastPosition = Position + directionLeftOfVehicle * 2f;

            for (var i = 0; i < 3; i++)
            {
                _cones.Add(new Cone(lastPosition));
                moveHeading *= 1.075f;
                lastPosition = lastPosition + MathHelper.ConvertHeadingToDirection(moveHeading) * 2f;
            }
        }

        public class Cone : IPreviewSupport
        {
            private Object _previewObject;

            public Cone(Vector3 position)
            {
                Position = position;
            }

            /// <summary>
            /// Get the position of the cone.
            /// </summary>
            public Vector3 Position { get; }

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