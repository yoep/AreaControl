using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RedirectTraffic
{
    public abstract class AbstractRedirectTraffic : IRedirectTraffic
    {
        #region IMenuComponent implementation

        /// <inheritdoc />
        public abstract UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public abstract bool IsVisible { get; }

        /// <inheritdoc />
        public abstract void OnMenuActivation(IMenu sender);
        
        /// <inheritdoc />
        public virtual void OnMenuHighlighted(IMenu sender)
        {
        }

        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        #endregion

        protected RedirectSlot DetermineRedirectSlot(float distanceFromPlayer)
        {
            var playerPosition = Game.LocalPlayer.Character.Position;
            var closestRoad = RoadUtil.GetClosestRoad(playerPosition, RoadType.All);

            //first lane = right side, last lane = left side
            var closestLane = GetClosestLaneToPlayer(playerPosition, closestRoad);
            var moveDirection = MathHelper.ConvertHeadingToDirection(RoadUtil.OppositeHeading(closestLane.Heading));

            return new RedirectSlot(closestLane.RightSide + moveDirection * distanceFromPlayer, closestLane.Heading);
        }

        private Road.Lane GetClosestLaneToPlayer(Vector3 playerPosition, Road road)
        {
            Road.Lane closestLane = null;
            var closestLaneDistance = 9999f;

            foreach (var lane in road.Lanes)
            {
                var laneDistanceFromPlayer = Vector3.Distance2D(playerPosition, lane.RightSide);

                if (laneDistanceFromPlayer > closestLaneDistance)
                    continue;

                closestLane = lane;
                closestLaneDistance = laneDistanceFromPlayer;
            }

            return closestLane;
        }
    }
}