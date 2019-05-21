using System;
using System.Linq;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RedirectTraffic
{
    public abstract class AbstractRedirectTraffic : IRedirectTraffic
    {
        private const float VehicleDistanceFromPlayer = 8f;

        #region IMenuComponent implementation

        /// <inheritdoc />
        public abstract UIMenuItem MenuItem { get; }
        
        /// <inheritdoc />
        public abstract MenuType Type { get; }

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public abstract bool IsVisible { get; }

        /// <inheritdoc />
        public abstract void OnMenuActivation(IMenu sender);

        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        #endregion

        protected static RedirectSlot DetermineRedirectSlot()
        {
            var initialPosition = GetInitialPosition();
            var closestRoad = RoadUtils.GetClosestRoad(initialPosition, RoadType.All);

            //first lane = right side, last lane = left side
            var closestLane = RoadUtils.GetClosestLane(closestRoad, initialPosition);
            var moveDirection = MathHelper.ConvertHeadingToDirection(RoadUtils.OppositeHeading(closestLane.Heading));
            var isLeftSideOfRoad = MultipleLanesInSameDirection(closestRoad, closestLane) && IsClosestToLeftLane(closestRoad, closestLane);
            var lanePosition = isLeftSideOfRoad ? closestLane.LeftSide : closestLane.RightSide;

            return new RedirectSlot(lanePosition + moveDirection * VehicleDistanceFromPlayer, closestLane.Heading, isLeftSideOfRoad);
        }

        private static Vector3 GetInitialPosition()
        {
            var player = Game.LocalPlayer.Character;
            var lastVehicle = player.LastVehicle;
            
            // use the players vehicle for determining the redirect traffic slot
            return lastVehicle != null ? lastVehicle.Position : player.Position;
        }

        private static bool IsClosestToLeftLane(Road road, Road.Lane lane)
        {
            var distanceRightSide = Vector3.Distance2D(road.RightSide, lane.Position);
            var distanceLeftSide = Vector3.Distance2D(road.LeftSide, lane.Position);

            return distanceLeftSide < distanceRightSide;
        }

        private static bool MultipleLanesInSameDirection(Road road, Road.Lane lane)
        {
            return road.Lanes
                .Where(x => x != lane)
                .Any(x => Math.Abs(lane.Heading - x.Heading) < 1f);
        }
    }
}