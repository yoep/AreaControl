using System;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Instances;
using Rage;

namespace AreaControl.Utils
{
    public static class VehicleUtils
    {
        private const float HeadingTolerance = 30f;
        private const float PositionTolerance = 5f;
        
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        /// <summary>
        /// Warp the vehicle into the given position/heading if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        public static void WarpVehicle(ACVehicle vehicle, AbstractVehicleSlot slot)
        {
            WarpVehicleInPosition(vehicle, slot);
            WarpVehicleInHeading(vehicle, slot);
        }

        /// <summary>
        /// Warp the vehicle into the slot heading if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        public static void WarpVehicleInHeading(ACVehicle vehicle, AbstractVehicleSlot slot)
        {
            var vehicleHeading = vehicle.Instance.Heading;
            var expectedHeading = slot.Heading;
            var headingDifference = Math.Abs(vehicleHeading - expectedHeading);

            Logger.Debug("Checking heading tolerance, expected: " + expectedHeading + ", actual: " + vehicleHeading + ", difference: " +
                         headingDifference);
            if (headingDifference > HeadingTolerance)
                vehicle.Instance.Heading = expectedHeading;
        }

        /// <summary>
        /// Warp the vehicle into the slot position if it's outside the allowed tolerance.
        /// </summary>
        /// <param name="vehicle">The vehicle to warp if needed.</param>
        /// <param name="slot">The slot to warp the vehicle into.</param>
        public static void WarpVehicleInPosition(ACVehicle vehicle, AbstractVehicleSlot slot)
        {
            var vehiclePosition = vehicle.Instance.Position;
            var expectedPosition = slot.Position;
            var positionDifference = Vector3.Distance(vehiclePosition, expectedPosition);

            Logger.Debug("Checking position tolerance, expected: " + expectedPosition + ", actual: " + vehiclePosition + ", difference: " +
                         positionDifference);
            if (positionDifference > PositionTolerance)
                vehicle.Instance.Position = expectedPosition;
        }
    }
}