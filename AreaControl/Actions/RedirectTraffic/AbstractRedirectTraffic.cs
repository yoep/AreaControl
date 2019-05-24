using System;
using System.Linq;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using AreaControl.Utils.Road;
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
        public abstract void OnMenuActivation(IMenu sender);

        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        #endregion

        protected static RedirectSlot DetermineRedirectSlot()
        {
            var playerPosition = GetInitialPosition();
            var closestRoad = RoadUtils.GetClosestRoad(playerPosition, RoadType.All);

            //first lane = right side, last lane = left side
            var closestLane = RoadUtils.GetClosestLane(closestRoad, playerPosition);
            var moveDirection = MathHelper.ConvertHeadingToDirection(RoadUtils.OppositeHeading(closestLane.Heading));
            var isLeftSideOfRoad = RoadUtils.HasMultipleLanesInSameDirection(closestRoad, closestLane) && RoadUtils.IsLeftSideLane(closestRoad, closestLane);
            var lanePosition = isLeftSideOfRoad ? closestLane.LeftSide : closestLane.RightSide;

            return new RedirectSlot(lanePosition + moveDirection * CalculateDistance(lanePosition, playerPosition), closestLane.Heading, isLeftSideOfRoad);
        }

        private static Vector3 GetInitialPosition()
        {
            var player = Game.LocalPlayer.Character;
            var lastVehicle = player.LastVehicle;
            
            // use the players vehicle for determining the redirect traffic slot
            return lastVehicle != null ? lastVehicle.Position : player.Position;
        }

        private static float CalculateDistance(Vector3 lanePosition, Vector3 playerPosition)
        {
            return Vector3.Distance2D(lanePosition, playerPosition) + VehicleDistanceFromPlayer;
        }
    }
}