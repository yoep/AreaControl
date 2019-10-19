using System;
using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Instances.Scenery;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.Model
{
    public class CrimeSceneSlot : IPreviewSupport
    {
        private const float DistanceBetweenBarriers = 6f;
        private const float DistanceBetweenCones = 3f;
        private const float DistanceFireTruckFromStart = 10f;
        private const float DistanceAmbulanceFromStart = 20f;

        private readonly List<ISceneryItem> _barriersWithCones = new List<ISceneryItem>();

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
        /// Get the police slot for the crime scene.
        /// </summary>
        public PoliceSlot Police { get; private set; }
        
        /// <summary>
        /// Get the firetruck slot for the crime scene.
        /// </summary>
        public FireTruckSlot Firetruck { get; private set; }
        
        /// <summary>
        /// Get the ambulance slot for the crime scene.
        /// </summary>
        public AmbulanceSlot Ambulance { get; private set; }
       
        /// <summary>
        /// Get the crime scene barriers.
        /// </summary>
        public IReadOnlyList<ISceneryItem> BarriersAndCones => _barriersWithCones.AsReadOnly();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => Police.IsPreviewActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            Police.CreatePreview();
            Firetruck.CreatePreview();
            Ambulance.CreatePreview();
            _barriersWithCones.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            Police.DeletePreview();
            Firetruck.DeletePreview();
            Ambulance.DeletePreview();
            _barriersWithCones.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region Functions

        private void Init()
        {
            InitializeBarriersAndCones();
            InitializePoliceSlot();
            InitializeFiretruckSlot();
            InitializeAmbulanceSlot();
        }

        private void InitializeBarriersAndCones()
        {
            var startDistance = 2.5f;
            var numberOfBarriers = Math.Floor((Vector3.Distance2D(StartPoint.Position, EndPoint.Position) - startDistance) / DistanceBetweenBarriers);
            var moveDirection = MathHelper.ConvertHeadingToDirection(StartLane.Heading);
            var lastPosition = IsLeftSideOfRoad ? StartLane.RightSide : StartLane.LeftSide;
            
            // set gap between car and first barrier
            lastPosition += moveDirection * startDistance;

            // create barriers in the length of the crime scene
            for(var i = 0; i < numberOfBarriers; i++)
            {
                var barrierPosition = lastPosition + moveDirection * DistanceBetweenBarriers;
                var conePosition = lastPosition + moveDirection * DistanceBetweenCones;

                _barriersWithCones.Add(new ConeWithLight(conePosition, StartLane.Heading + 90f));
                _barriersWithCones.Add(new Barrier(barrierPosition, StartLane.Heading + 90f));
                lastPosition = barrierPosition;
            }
        }

        private void InitializePoliceSlot()
        {
            // create a police slot at the start lane of the crime scene
            Police = new PoliceSlot(StartLane.Position, StartLane.Heading);
        }

        private void InitializeFiretruckSlot()
        {
            var startPosition = IsLeftSideOfRoad ? StartLane.LeftSide : StartLane.RightSide;
            var position = startPosition + MathHelper.ConvertHeadingToDirection(StartLane.Heading) * DistanceFireTruckFromStart;
            
            Firetruck = new FireTruckSlot(position, StartLane.Heading - 30f);
        }

        private void InitializeAmbulanceSlot()
        {
            var startPosition = IsLeftSideOfRoad ? StartLane.LeftSide : StartLane.RightSide;
            var position = startPosition + MathHelper.ConvertHeadingToDirection(StartLane.Heading) * DistanceAmbulanceFromStart;
            
            Ambulance = new AmbulanceSlot(position, StartLane.Heading - 30f);
        }

        #endregion
    }
}