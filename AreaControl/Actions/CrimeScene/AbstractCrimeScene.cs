using System;
using System.Linq;
using AreaControl.Actions.Model;
using AreaControl.Instances;
using AreaControl.Utils.Road;
using Rage;

namespace AreaControl.Actions.CrimeScene
{
    public abstract class AbstractCrimeScene
    {
        private const float DistanceFromPlayer = 15f;
        private const float SpawnDistance = 150f;

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

        protected Vector3 GetSpawnPositionPolice(Vector3 position, float crimeSceneHeading)
        {
           return GetSpawnPosition(position, crimeSceneHeading, 0f);
        }
        
        protected Vector3 GetSpawnPositionFireTruck(Vector3 position, float crimeSceneHeading)
        {
           return GetSpawnPosition(position, crimeSceneHeading, 10f);
        }
        
        protected Vector3 GetSpawnPositionAmbulance(Vector3 position, float crimeSceneHeading)
        {
           return GetSpawnPosition(position, crimeSceneHeading, 20f);
        }

        private Vector3 GetSpawnPosition(Vector3 position, float crimeSceneHeading, float distance)
        {
            return GetPositionBehindCrimeScene(position, crimeSceneHeading).Position + MathHelper.ConvertHeadingToDirection(crimeSceneHeading) * distance;
        }

        private Road GetRoad(Vector3 position)
        {
            return RoadUtils.GetClosestRoad(position, RoadType.All);
        }

        private Road.Lane GetPositionBehindCrimeScene(Vector3 position, float crimeSceneHeading)
        {
            var oppositeHeading = RoadUtils.OppositeHeading(crimeSceneHeading);
            var oppositeDirection = MathHelper.ConvertHeadingToDirection(oppositeHeading);
            var road = RoadUtils.GetClosestRoad(position + oppositeDirection * SpawnDistance, RoadType.MajorRoadsOnly);
            var result = road.Lanes.First();

            foreach (var lane in road.Lanes)
            {
                if (Math.Abs(lane.Heading - crimeSceneHeading) < 10)
                {
                    result = lane;
                }
            }

            return result;
        }

        #endregion
    }
}