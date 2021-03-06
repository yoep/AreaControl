using System;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Instances;
using Rage;

namespace AreaControl.Utils
{
    public static class VehicleUtils
    {
        private const float HeadingTolerance = 25f;
        private const float PositionTolerance = 5f;

        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        /// <summary>
        /// Warp the vehicle into the given position/heading if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        /// <param name="headingTolerance">The heading tolerance the vehicle may have.</param>
        /// <param name="positionTolerance">The distance tolerance the vehicle may have.</param>
        public static void WarpVehicle(ACVehicle vehicle, IVehicleSlot slot, 
            float headingTolerance = HeadingTolerance, float positionTolerance = PositionTolerance)
        {
            WarpVehicleInHeading(vehicle, slot, headingTolerance);
            WarpVehicleInPosition(vehicle, slot, positionTolerance);
        }

        /// <summary>
        /// Warp the vehicle into the slot heading if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        /// <param name="tolerance">The heading tolerance the vehicle may have.</param>
        public static void WarpVehicleInHeading(ACVehicle vehicle, IVehicleSlot slot, float tolerance = HeadingTolerance)
        {
            var vehicleHeading = vehicle.Instance.Heading;
            var expectedHeading = slot.Heading;
            var headingDifference = Math.Abs(vehicleHeading - expectedHeading);

            Logger.Trace("Checking heading tolerance, expected: " + expectedHeading + ", actual: " + vehicleHeading + ", difference: " +
                         headingDifference);
            if (headingDifference > tolerance)
                vehicle.Instance.Heading = expectedHeading;
        }

        /// <summary>
        /// Warp the vehicle into the slot position if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        /// <param name="tolerance">The distance tolerance the vehicle may have.</param>
        public static void WarpVehicleInPosition(ACVehicle vehicle, IVehicleSlot slot, float tolerance = PositionTolerance)
        {
            var vehicleInstance = vehicle.Instance;
            var vehiclePosition = vehicleInstance.Position;
            var expectedPosition = slot.Position;
            var positionDifference = Math.Abs(Vector3.Distance2D(vehiclePosition, expectedPosition));

            Logger.Trace("Checking position tolerance, expected: " + expectedPosition + ", actual: " + vehiclePosition + ", difference: " +
                         positionDifference);
            if (!(positionDifference > tolerance)) 
                return;
            
            Logger.Trace("Warping vehicle & placing on the ground");
            vehicleInstance.Position = GameUtils.GetOnTheGroundVector(expectedPosition);
        }
    }
}