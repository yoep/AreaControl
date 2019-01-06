using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public static class RoadUtil
    {
        #region Methods

        /// <summary>
        /// Get the closest road near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static Road GetClosestRoad(Vector3 position, RoadType roadType)
        {
            return GetNearbyRoads(position, roadType).First();
        }

        /// <summary>
        /// Get nearby roads near the given position.
        /// </summary>
        /// <param name="position">Set the position to use as reference.</param>
        /// <param name="roadType">Set the road type.</param>
        /// <returns>Returns the position of the closest road.</returns>
        public static IEnumerable<Road> GetNearbyRoads(Vector3 position, RoadType roadType)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(roadType, "roadType cannot be null");
            Vector3 road1;
            Vector3 road2;
            int p7;
            int p8;
            float junctionIndication;

            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out road1, out road2, out p7, out p8, out junctionIndication,
                (int) roadType);
            var rage = IoC.Instance.GetInstance<IRage>();
            rage.LogTrivialDebug("p7=" + p7);
            rage.LogTrivialDebug("p8=" + p8);
            rage.LogTrivialDebug("p9=" + junctionIndication);

            return new List<Road>
            {
                RoadBuilder.Builder()
                    .Position(road1)
                    .IsAtJunction((int) junctionIndication != 0)
                    .Lanes(DiscoverLanes(road1))
                    .Build(),
                RoadBuilder.Builder()
                    .Position(road2)
                    .IsAtJunction((int) junctionIndication != 0)
                    .Lanes(DiscoverLanes(road2))
                    .Build()
            };
        }

        #endregion

        #region Functions

        private static List<Road.Lane> DiscoverLanes(Vector3 roadPosition)
        {
            var currentPosition = roadPosition;
            var initialHeading = GetHeading(currentPosition);
            var rightSideEndPoint = GetLastPointOnTheLane(currentPosition, initialHeading - 90f);
            var leftSideEndPoint = GetLastPointOnTheLane(currentPosition, initialHeading + 90f);

            return new List<Road.Lane>
            {
                LaneBuilder.Builder()
                    .Heading(initialHeading)
                    .RightSide(rightSideEndPoint)
                    .LeftSide(leftSideEndPoint)
                    .Width(GetWidth(rightSideEndPoint, roadPosition))
                    .Build(),
                //TODO: fix multi lane support
                /*LaneBuilder.Builder()
                    .Heading(initialHeading + 180f)
                    .RightSide(roadPosition)
                    .LeftSide(leftSideEndPoint)
                    .Width(GetWidth(roadPosition, leftSideEndPoint))
                    .Build()*/
            };
        }

        private static float GetHeading(Vector3 position)
        {
            Vector3 roadPosition;
            float heading;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out roadPosition, out heading, 1, 3, 0);

            return MathHelper.NormalizeHeading(heading);
        }

        private static Vector3 GetLastPointOnTheLane(Vector3 position, float heading)
        {
            var currentPosition = position;
            var direction = MathHelper.ConvertHeadingToDirection(heading);
            bool isPointOnRoad;
            Vector3 lastPositionOnTheRoad;

            do
            {
                lastPositionOnTheRoad = currentPosition;
                currentPosition = currentPosition + direction * 0.25f;
                isPointOnRoad = NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(currentPosition.X, currentPosition.Y, currentPosition.Z);
            } while (isPointOnRoad);

            return lastPositionOnTheRoad;
        }

        private static float GetWidth(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        #endregion
    }

    internal class RoadBuilder
    {
        private readonly List<Road.Lane> _lanes = new List<Road.Lane>();
        private Vector3 _position;
        private bool _isAtJunction;

        private RoadBuilder()
        {
        }

        public static RoadBuilder Builder()
        {
            return new RoadBuilder();
        }

        public RoadBuilder Position(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            return this;
        }

        public RoadBuilder IsAtJunction(bool value)
        {
            _isAtJunction = value;
            return this;
        }

        public RoadBuilder Lanes(List<Road.Lane> lanes)
        {
            Assert.NotNull(lanes, "lanes cannot be null");
            _lanes.AddRange(lanes);
            return this;
        }

        public RoadBuilder AddLane(Road.Lane lane)
        {
            Assert.NotNull(lane, "lane cannot be null");
            _lanes.Add(lane);
            return this;
        }

        public Road Build()
        {
            return new Road(_position, _lanes.AsReadOnly(), _isAtJunction);
        }
    }

    internal class LaneBuilder
    {
        private float _heading;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private float _width;

        private LaneBuilder()
        {
        }

        public static LaneBuilder Builder()
        {
            return new LaneBuilder();
        }

        public LaneBuilder Heading(float heading)
        {
            Assert.NotNull(heading, "heading cannot be null");
            _heading = heading;
            return this;
        }

        public LaneBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public LaneBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public LaneBuilder Width(float width)
        {
            Assert.NotNull(width, "width cannot be null");
            _width = width;
            return this;
        }

        public Road.Lane Build()
        {
            return new Road.Lane(_heading, _rightSide, _leftSide, _width);
        }
    }
}