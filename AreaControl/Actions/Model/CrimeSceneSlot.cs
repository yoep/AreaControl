using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Instances.Scenery;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.Model
{
    public class CrimeSceneSlot : IPreviewSupport
    {
        private const float DistanceBetweenBarriers = 3f;
        private const float DistanceFireTruckFromStart = 10f;
        private const float DistanceAmbulanceFromStart = 20f;

        private readonly List<Barrier> _barriers = new List<Barrier>();
        private BlockSlot _blockSlot;

        public CrimeSceneSlot(Road startPoint, Road endPoint, Vector3 originalPlayerPosition)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            OriginalPlayerPosition = originalPlayerPosition;

            StartLane = RoadUtils.GetClosestLane(StartPoint, OriginalPlayerPosition);
            EndLane = RoadUtils.GetClosestLane(EndPoint, OriginalPlayerPosition);
            IsLeftSideOfRoad = RoadUtils.HasMultipleLanesInSameDirection(StartPoint, StartLane) && RoadUtils.IsLeftSideLane(StartPoint, StartLane);

            Init();
        }

        #region Properties

        /// <summary>
        /// Get the start lane of the crime scene.
        /// </summary>
        public Road StartPoint { get; }

        /// <summary>
        /// Get the start lane of the crime scene.
        /// </summary>
        public Road.Lane StartLane { get; }

        /// <summary>
        /// Get the end lane of the crime scene.
        /// </summary>
        public Road EndPoint { get; }

        /// <summary>
        /// Get the end lane of the crime scene.
        /// </summary>
        public Road.Lane EndLane { get; }

        /// <summary>
        /// Get the original player position when this crime scene slot had been created.
        /// </summary>
        public Vector3 OriginalPlayerPosition { get; }

        /// <summary>
        /// Get if the crime scene should be created on the left side.
        /// </summary>
        public bool IsLeftSideOfRoad { get; }
        
        /// <summary>
        /// Get the firetruck slot for the crime scene.
        /// </summary>
        public FireTruckSlot Firetruck { get; private set; }
        
        /// <summary>
        /// Get the ambulance slot for the crime scene.
        /// </summary>
        public AmbulanceSlot Ambulance { get; private set; }
       
        public IReadOnlyList<Barrier> Barriers => _barriers.AsReadOnly();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _blockSlot.IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _blockSlot.CreatePreview();
            Firetruck.CreatePreview();
            Ambulance.CreatePreview();
            _barriers.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _blockSlot.DeletePreview();
            Firetruck.DeletePreview();
            Ambulance.DeletePreview();
            _barriers.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region Functions

        private void Init()
        {
            InitializeBlockSlot();
            InitializeBarriers();
            InitializeFiretruckSlot();
            InitializeAmbulanceSlot();
        }

        private void InitializeBlockSlot()
        {
            // create a block slot at the start lane of the crime scene
            _blockSlot = new BlockSlot(StartLane.Position, StartLane.Heading);
        }

        private void InitializeBarriers()
        {
            var sceneDistance = Vector3.Distance(StartPoint.Position, EndPoint.Position);
            var moveDirection = MathHelper.ConvertHeadingToDirection(StartLane.Heading);
            var lastPosition = IsLeftSideOfRoad ? StartLane.RightSide : StartLane.LeftSide;

            // create barriers in the length of the crime scene
            for (var i = 0; i < sceneDistance / DistanceBetweenBarriers; i++)
            {
                var position = lastPosition + moveDirection * DistanceBetweenBarriers;

                _barriers.Add(new Barrier(position, StartLane.Heading + 90f));
                lastPosition = position;
            }
        }

        private void InitializeFiretruckSlot()
        {
            var position = StartLane.Position + MathHelper.ConvertHeadingToDirection(StartLane.Heading) * DistanceFireTruckFromStart;
            
            Firetruck = new FireTruckSlot(position, StartLane.Heading - 30f);
        }

        private void InitializeAmbulanceSlot()
        {
            var position = StartLane.Position + MathHelper.ConvertHeadingToDirection(StartLane.Heading) * DistanceAmbulanceFromStart;
            
            Ambulance = new AmbulanceSlot(position, StartLane.Heading - 30f);
        }

        #endregion
    }
}