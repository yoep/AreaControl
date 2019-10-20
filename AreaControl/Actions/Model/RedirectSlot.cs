using System.Collections.Generic;
using AreaControl.Instances.Scenery;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.Model
{
    public class RedirectSlot : AbstractVehicleSlot
    {
        private const float PedDistanceFromVehicle = 3f;
        private readonly List<Entity> _previewObjects = new List<Entity>();
        private readonly List<Cone> _cones = new List<Cone>();

        public RedirectSlot(Vector3 position, float heading, bool placeConesOnRightSide)
            : base(position, heading)
        {
            PedHeading = RoadUtils.OppositeHeading(Heading);
            PedPosition = Position + MathHelper.ConvertHeadingToDirection(PedHeading) * PedDistanceFromVehicle;
            PlaceConesRightSide = placeConesOnRightSide;

            Init();
        }

        #region Properties

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
        public override void CreatePreview()
        {
            if (IsPreviewActive)
                return;
            
            base.CreatePreview();

            _previewObjects.Add(new Ped(new Rage.Model("s_m_y_cop_01"), PedPosition, PedHeading));
            _previewObjects.ForEach(PreviewUtils.TransformToPreview);
            _cones.ForEach(x => x.CreatePreview());
            Sign.CreatePreview();
            SignLight.CreatePreview();
        }

        /// <inheritdoc />
        public override void DeletePreview()
        {
            if (!IsPreviewActive)
                return;
            
            base.DeletePreview();

            _previewObjects.ForEach(EntityUtils.Remove);
            _previewObjects.Clear();
            _cones.ForEach(x => x.DeletePreview());
            Sign.DeletePreview();
            SignLight.DeletePreview();
        }

        #endregion

        #region Methods

        protected override Rage.Model GetModelInstance()
        {
            return ModelUtils.GetLocalPolice(Position);
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
            var positionLeftOfVehicle = GameUtils.GetOnTheGroundVector(Position + directionLeftOfVehicle * 2f);
            var secondCone = GameUtils.GetOnTheGroundVector(positionLeftOfVehicle + directionBehindVehicle * 2f);
            var thirdCone = GameUtils.GetOnTheGroundVector(secondCone + MathHelper.ConvertHeadingToDirection(moveHeading + headingThirdCone) * 2f);

            _cones.Add(new Cone(positionLeftOfVehicle, moveHeading));
            _cones.Add(new Cone(secondCone, moveHeading));
            _cones.Add(new Cone(thirdCone, moveHeading));
        }

        private void CreateSign()
        {
            var headingBehindVehicle = PedHeading;
            var directionBehindVehicle = MathHelper.ConvertHeadingToDirection(headingBehindVehicle + 10f);
            var positionSign = GameUtils.GetOnTheGroundVector(Position + directionBehindVehicle * 8f);
            var positionLight = GameUtils.GetOnTheGroundVector(positionSign + directionBehindVehicle * 2f);

            Sign = new StoppedVehiclesSign(positionSign, RoadUtils.OppositeHeading(headingBehindVehicle));
            SignLight = new GroundFloodLight(positionLight, headingBehindVehicle);
        }

        #endregion
    }
}