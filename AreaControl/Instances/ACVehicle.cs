using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines a <see cref="Vehicle"/> which is managed by the AreaControl plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACVehicle : IACEntity
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

        /// <summary>
        /// Empty this vehicle.
        /// Makes all occupants of this vehicle leave it.
        /// </summary>
        public TaskExecutor Empty()
        {
            return TaskUtil.EmptyVehicle(Instance);
        }

        /// <inheritdoc />
        public void Delete()
        {
            Instance.Delete();
        }

        /// <summary>
        /// Enables the emergency lights with sound.
        /// </summary>
        public void EnableSirens()
        {
            Instance.IsSirenOn = true;
            Instance.IsSirenSilent = false;
        }
        
        /// <summary>
        /// Enables the emergency lights without sound.
        /// </summary>
        public void EnableEmergencyLights()
        {
            Instance.IsSirenOn = true;
            Instance.IsSirenSilent = true;
        }
    }
}