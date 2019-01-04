using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public static class TaskUtil
    {
        /// <summary>
        /// Empty all occupants of the given vehicle.
        /// </summary>
        /// <param name="vehicle">Set the vehicle to empty.</param>
        public static void EmptyVehicle(Vehicle vehicle)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.TASK_EVERYONE_LEAVE_VEHICLE(vehicle);
        }
    }
}