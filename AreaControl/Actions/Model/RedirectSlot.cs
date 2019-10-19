using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Instances.Scenery;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.Model
{
    public class RedirectSlot : IPreviewSupport
    {
        private const float PedDistanceFromVehicle = 3f;
        private readonly List<Entity> _previewObjects = new List<Entity>();
        private readonly List<Cone> _cones = new List<Cone>();

        public RedirectSlot(Vector3 position, float heading, bool placeConesOnRightSide)
        {
            Position = position;
            Heading = heading;
            PedHeading = RoadUtils.OppositeHeading(Heading);
            PedPosition = Position + MathHelper.ConvertHeadingToDirection(PedHeading) * PedDistanceFromVehicle;
            PlaceConesRightSide = placeConesOnRightSide;

            Init();
        }

        #region Properties

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
        /// Get if the cones should be placed on the right side of the vehicle.
        /// </summary>
        public bool PlaceConesRightSide { get; }

        /// <summary>
        /// Get the cones to place for this redirect traffic slot.
        /// </summary>
        public IReadOnlyList<Cone> Cones => _cones.AsReadOnly();

        /// <summary>
        /// Get the stopped vehicles sign to place for this redirect traffic slot.
        /// </summary>
        public StoppedVehiclesSign Sign { get; private set; }

        /// <summary>
        /// Get the light for the sign.
        /// </summary>
        public GroundFloodLight SignLight { get; private set; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObjects.Count > 0;

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            _previewObjects.Add(new Vehicle("POLICE", Position, Heading));
            _previewObjects.Add(new Ped(new Rage.Model("s_m_y_cop_01"), PedPosition, PedHeading));
            _previewObjects.ForEach(PreviewUtils.TransformToPreview);
            _cones.ForEach(x => x.CreatePreview());
            Sign.CreatePreview();
            SignLight.CreatePreview();
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            _previewObjects.ForEach(EntityUtils.Remove);
            _previewObjects.Clear();
            _cones.ForEach(x => x.DeletePreview());
            Sign.DeletePreview();
            SignLight.DeletePreview();
        }

        #endregion

        #region Methods

        public void ClearSlotFromTraffic()
        {
            EntityUtils.CleanArea(Position, 5f, true);
        }

        public override string ToString()
        {
            return
                $"{nameof(Position)}: {Position}, {nameof(Heading)}: {Heading}, {nameof(PedPosition)}: {PedPosition}, {nameof(PedHeading)}: {PedHeading}, " +
                $"{nameof(IsPreviewActive)}: {IsPreviewActive}";
        }

        #endregion

        #region Functions

        private void Init()
        {
            CreateCones();
            CreateSign();
        }

        private void CreateCones()
        {
            var headingToMove = PlaceConesRightSide ? -90f : 90f;
            var headingThirdCone = PlaceConesRightSide ? -25f : 25f;
            var directionLeftOfVehicle = MathHelper.ConvertHeadingToDirection(Heading + headingToMove);
            var moveHeading = PedHeading;
            var directionBehindVehicle = MathHelper.ConvertHeadingToDirection(moveHeading);
            var positionLeftOfVehicle = Position + directionLeftOfVehicle * 2f;
            var secondCone = positionLeftOfVehicle + directionBehindVehicle * 2f;
            var thirdCone = secondCone + MathHelper.ConvertHeadingToDirection(moveHeading + headingThirdCone) * 2f;

            _cones.Add(new Cone(positionLeftOfVehicle, moveHeading));
            _cones.Add(new Cone(secondCone, moveHeading));
            _cones.Add(new Cone(thirdCone, moveHeading));
        }

        private void CreateSign()
        {
            var headingBehindVehicle = PedHeading;
            var directionBehindVehicle = MathHelper.ConvertHeadingToDirection(headingBehindVehicle + 10f);
            var positionSign = Position + directionBehindVehicle * 8f;
            var positionLight = positionSign + directionBehindVehicle * 2f;

            Sign = new StoppedVehiclesSign(positionSign, RoadUtils.OppositeHeading(headingBehindVehicle));
            SignLight = new GroundFloodLight(positionLight, headingBehindVehicle);
        }

        #endregion
    }
}