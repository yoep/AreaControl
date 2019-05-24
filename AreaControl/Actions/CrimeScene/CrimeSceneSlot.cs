using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Instances.Scenery;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeSceneSlot : IPreviewSupport
    {
        private const float DistanceBetweenBarriers = 3f;

        private readonly List<Barrier> _barriers = new List<Barrier>();

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

        public Road.Lane StartLane { get; }

        /// <summary>
        /// Get the end lane of the crime scene.
        /// </summary>
        public Road EndPoint { get; }

        public Road.Lane EndLane { get; }

        /// <summary>
        /// Get the original player position when this crime scene slot had been created.
        /// </summary>
        public Vector3 OriginalPlayerPosition { get; }

        /// <summary>
        /// Get if the crime scene should be created on the left side.
        /// </summary>
        public bool IsLeftSideOfRoad { get; }

        public IReadOnlyList<Barrier> Barriers => _barriers.AsReadOnly();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            _barriers.ForEach(x => x.CreatePreview());
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _barriers.ForEach(x => x.DeletePreview());
        }

        #endregion

        #region Functions

        private void Init()
        {
            var sceneDistance = Vector3.Distance(StartPoint.Position, EndPoint.Position);
            var moveDirection = MathHelper.ConvertHeadingToDirection(StartLane.Heading);
            var lastPosition = IsLeftSideOfRoad ? StartLane.RightSide : StartLane.LeftSide;
            
            for (var i = 0; i < sceneDistance / DistanceBetweenBarriers; i++)
            {
                var position = lastPosition + moveDirection * DistanceBetweenBarriers;
                
                _barriers.Add(new Barrier(position, StartLane.Heading + 90f));
                lastPosition = position;
            }
        }

        #endregion
    }
}