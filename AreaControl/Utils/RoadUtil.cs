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
            int numberOfLanes1;
            int numberOfLanes2;
            float junctionIndication;

            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out road1, out road2, out numberOfLanes1, out numberOfLanes2,
                out junctionIndication, (int) roadType);
            var rage = IoC.Instance.GetInstance<IRage>();
            rage.LogTrivialDebug("numberOfLanes1=" + numberOfLanes1);
            rage.LogTrivialDebug("numberOfLanes2=" + numberOfLanes2);
            rage.LogTrivialDebug("junctionIndication=" + junctionIndication);

            return new List<Road>
            {
                DiscoverRoad(road1, numberOfLanes1, numberOfLanes2, junctionIndication),
                DiscoverRoad(road2, numberOfLanes1, numberOfLanes2, junctionIndication)
            };
        }

        #endregion

        #region Functions

        private static Road DiscoverRoad(Vector3 roadPosition, int numberOfLanes1, int numberOfLanes2, float junctionIndication)
        {
            var rightSideHeading = GetHeading(roadPosition);
            var roadRightSide = GetLastPointOnTheLane(roadPosition, rightSideHeading - 90f);
            var roadLeftSide = GetLastPointOnTheLane(roadPosition, rightSideHeading + 90f);
            var singleDirection = IsSingleDirectionRoad(numberOfLanes1, numberOfLanes2);

            //Fix a side if it's the same as the middle of the road as GetLastPointOnTheLane probably failed to determine the last point
            if (roadRightSide == roadPosition)
                roadRightSide = FixFailedLastPointCalculation(roadPosition, roadLeftSide, rightSideHeading - 90f);
            if (roadLeftSide == roadPosition)
                roadLeftSide = FixFailedLastPointCalculation(roadPosition, roadRightSide, rightSideHeading + 90f);

            return RoadBuilder.Builder()
                .Position(roadPosition)
                .RightSide(roadRightSide)
                .LeftSide(roadLeftSide)
                .IsAtJunction((int) junctionIndication != 0)
                .IsSingleDirection(singleDirection)
                .Lanes(DiscoverLanes(roadRightSide, roadLeftSide, rightSideHeading, numberOfLanes1, numberOfLanes2))
                .Build();
        }

        private static List<Road.Lane> DiscoverLanes(Vector3 roadRightSide, Vector3 roadLeftSide, float rightSideHeading, int numberOfLanes1,
            int numberOfLanes2)
        {
            var singleDirection = IsSingleDirectionRoad(numberOfLanes1, numberOfLanes2);
            var lanes = new List<Road.Lane>();

            if (singleDirection)
            {
                var numberOfLanes = numberOfLanes1 == 0 ? numberOfLanes2 : numberOfLanes1;
                var laneWidth = GetWidth(roadRightSide, roadLeftSide) / numberOfLanes;
                var lastRightPosition = roadRightSide;
                var moveDirection = MathHelper.ConvertHeadingToDirection(rightSideHeading + 90f);

                for (var index = 1; index <= numberOfLanes; index++)
                {
                    var laneLeftPosition = lastRightPosition + moveDirection * laneWidth;
                    lanes.Add(LaneBuilder.Builder()
                        .Number(index)
                        .Heading(rightSideHeading)
                        .RightSide(lastRightPosition)
                        .LeftSide(laneLeftPosition)
                        .Width(laneWidth)
                        .Build());
                    lastRightPosition = laneLeftPosition;
                }
            }
            else
            {
                
            }

            return lanes;
        }

        // This fix is a simple workaround if the last point detection failed with the native function of GTA
        // It will assume that both sides have the same width and mirror the width to the other side to determine the point
        private static Vector3 FixFailedLastPointCalculation(Vector3 roadPosition, Vector3 otherSidePosition, float heading)
        {
            var widthOtherSide = Vector3.Distance2D(roadPosition, otherSidePosition);
            var directionOfTheSideToFix = MathHelper.ConvertHeadingToDirection(heading);
            return roadPosition + directionOfTheSideToFix * widthOtherSide;
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
                currentPosition = currentPosition + direction * 0.5f;
                isPointOnRoad = NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(currentPosition.X, currentPosition.Y, currentPosition.Z);
            } while (isPointOnRoad);

            return lastPositionOnTheRoad;
        }

        private static float GetWidth(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }

        private static bool IsSingleDirectionRoad(int numberOfLanes1, int numberOfLanes2)
        {
            return numberOfLanes1 == 0 || numberOfLanes2 == 0;
        }

        #endregion
    }

    internal class RoadBuilder
    {
        private readonly List<Road.Lane> _lanes = new List<Road.Lane>();
        private Vector3 _position;
        private Vector3 _rightSide;
        private Vector3 _leftSide;
        private bool _isAtJunction;
        private bool _isSingleDirection;

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

        public RoadBuilder RightSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _rightSide = position;
            return this;
        }

        public RoadBuilder LeftSide(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _leftSide = position;
            return this;
        }

        public RoadBuilder IsAtJunction(bool value)
        {
            _isAtJunction = value;
            return this;
        }

        public RoadBuilder IsSingleDirection(bool value)
        {
            _isSingleDirection = value;
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
            Assert.NotNull(_position, "position has not been set");
            return new Road(_position, _rightSide, _leftSide, _lanes.AsReadOnly(), _isAtJunction, _isSingleDirection);
        }
    }

    internal class LaneBuilder
    {
        private int _number;
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

        public LaneBuilder Number(int number)
        {
            _number = number;
            return this;
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
            return new Road.Lane(_number, _heading, _rightSide, _leftSide, _width);
        }
    }
}