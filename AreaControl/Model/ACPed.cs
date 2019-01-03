using Rage;

namespace AreaControl.Model
{
    /// <summary>
    /// Defines a <see cref="Ped"/> which is managed by the AreaControl plugin.
    /// </summary>
    public class ACPed
    {
        public ACPed(Ped instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Get the GTA V Ped instance.
        /// </summary>
        public Ped Instance { get; }

        /// <summary>
        /// Get or set if this vehicle is busy.
        /// If not busy, this vehicle can be used for a task by the plugin.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Warp this ped into a vehicle.
        /// </summary>
        /// <param name="vehicle">Set the vehicle to warp the ped into.</param>
        /// <param name="seat">Set the seat to place the ped in.</param>
        public void WarpIntoVehicle(ACVehicle vehicle, VehicleSeat seat)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            
            Instance.WarpIntoVehicle(vehicle.Instance, (int) seat);
        }
    }
}