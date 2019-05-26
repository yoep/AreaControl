using AreaControl.Actions.Model;
using AreaControl.Instances;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.CrimeScene
{
    public abstract class AbstractCrimeScene
    {
        private const float DistanceFromPlayer = 15f;

        #region Properties

        /// <summary>
        /// Get or set if this action is active.
        /// </summary>
        protected bool IsActive { get; set; }

        #endregion

        #region Functions

        protected CrimeSceneSlot DetermineCrimeSceneSlot()
        {
            var playerPosition = Game.LocalPlayer.Character.Position;
            var closestRoad = RoadUtils.GetClosestRoad(playerPosition, RoadType.All);

            //first lane = right side, last lane = left side
            var closestLane = RoadUtils.GetClosestLane(closestRoad, playerPosition);
            var laneDirection = MathHelper.ConvertHeadingToDirection(closestLane.Heading);
            var oppositeLaneDirection = MathHelper.ConvertHeadingToDirection(RoadUtils.OppositeHeading(closestLane.Heading));
            var pointBehindPlayer = GetRoad(closestLane.Position + oppositeLaneDirection * DistanceFromPlayer);
            var pointInFrontOfPlayer = GetRoad(closestLane.Position + laneDirection * DistanceFromPlayer);

            return new CrimeSceneSlot(pointBehindPlayer, pointInFrontOfPlayer, playerPosition);
        }

        private Road GetRoad(Vector3 position)
        {
            return RoadUtils.GetClosestRoad(position, RoadType.All);
        }

        #endregion
    }
}