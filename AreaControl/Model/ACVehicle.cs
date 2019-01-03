using System.Collections.Generic;
using Rage;

namespace AreaControl.Model
{
    /// <summary>
    /// Defines a <see cref="Vehicle"/> which is managed by the AreaControl plugin.
    /// </summary>
    public class ACVehicle
    {
        public ACVehicle(Vehicle instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Get the GTA V vehicle instance.
        /// </summary>
        public Vehicle Instance { get; }

        /// <summary>
        /// Get or set the Area Controlled passengers of this vehicle.
        /// Will return an empty list when no passengers are available in this vehicle.
        /// </summary>
        public List<ACPed> Passengers { get; } = new List<ACPed>();

        /// <summary>
        /// Get the Area Controlled occupants (passengers + driver) of this vehicle.
        /// Will return an empty list if no occupants are present in the vehicle.
        /// </summary>
        public List<ACPed> Occupants
        {
            get
            {
                var occupants = new List<ACPed>();
                if (Driver != null)
                    occupants.Add(Driver);
                if (Passengers != null)
                    occupants.AddRange(Passengers);
                return occupants;
            }
        }

        /// <summary>
        /// Get or set the Area Controller driver of this vehicle.
        /// </summary>
        public ACPed Driver { get; set; }

        /// <summary>
        /// Get or set if this vehicle is busy.
        /// If not busy, this vehicle can be used for a task by the plugin.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Create a blip in the map for this vehicle.
        /// </summary>
        public void CreateBlip()
        {
           new Blip(Instance)
            {
                IsRouteEnabled = false,
                IsFriendly = true
            };
        }
    }
}