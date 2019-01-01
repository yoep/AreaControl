using AreaControl.Model;
using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public class RoadUtil : IRoadUtil
    {
        #region Methods

        /// <inheritdoc />
        public Road GetClosestRoad(Vector3 position, RoadType roadType)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(roadType, "roadType cannot be null");
            Vector3 closestRoad;
            Vector3 p6;
            int p7;
            int p8;
            float p9;

            NativeFunction.Natives.GET_CLOSEST_ROAD(position.X, position.Y, position.Z, 1f, 1, out closestRoad, out p6, out p7, out p8, out p9, (int) roadType);

            var road = new Road(closestRoad, GetRoadHeading(closestRoad));
            road.RightSide = GetRoadRightSide(road);
            road.LeftSide = GetRoadLeftSide(road);
            road.Width = GetRoadWidth(road);
            return road;
        }

        #endregion

        #region Functions

        private static float GetRoadHeading(Vector3 position)
        {
            Vector3 roadPosition;
            float heading;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out roadPosition, out heading, 1, 3, 0);

            return MathHelper.NormalizeHeading(heading);
        }

        private static Vector3 GetRoadRightSide(Road road)
        {
            return GetLastPointOnTheRoad(road.Position, MathHelper.ConvertHeadingToDirection(road.Heading - 90f));
        }

        private static Vector3 GetRoadLeftSide(Road road)
        {
            return GetLastPointOnTheRoad(road.Position, MathHelper.ConvertHeadingToDirection(road.Heading + 90f));
        }

        private static Vector3 GetLastPointOnTheRoad(Vector3 position, Vector3 direction)
        {
            var currentPosition = position;
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

        private static float GetRoadWidth(Road road)
        {
            var roadLeftSide = road.LeftSide;
            var roadRightSide = road.RightSide;

            return Vector3.Distance(roadLeftSide, roadRightSide);
        }

        #endregion
    }
}