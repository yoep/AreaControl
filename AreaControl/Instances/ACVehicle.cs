using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private Blip _blip;

        public ACVehicle(Vehicle instance, long id)
        {
            Instance = instance;
            Id = id;
        }
        
        /// <inheritdoc />
        public long Id { get; }

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
        public bool IsBusy => Driver.IsBusy;

        /// <summary>
        /// Create a blip in the map for this vehicle.
        /// </summary>
        public void CreateBlip()
        {
            _blip = new Blip(Instance)
            {
                IsRouteEnabled = false,
                IsFriendly = true
            };
        }

        /// <summary>
        /// Delete the blip from this vehicle.
        /// </summary>
        public void DeleteBlip()
        {
            if (_blip != null)
                _blip.Delete();
        }

        /// <summary>
        /// Empty this vehicle.
        /// Makes all occupants of this vehicle leave it.
        /// </summary>
        public TaskExecutor Empty()
        {
            Occupants.ForEach(x => x.IsBusy = true);
            var executor = TaskUtil.EmptyVehicle(Instance);
            executor.OnCompletion += (sender, args) => Occupants.ForEach(x => x.IsBusy = false);
            return executor;
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
        /// Disables the emergency lights with sound.
        /// </summary>
        public void DisableSirens()
        {
            Instance.IsSirenOn = false;
        }

        /// <summary>
        /// Enables the emergency lights without sound.
        /// </summary>
        public void EnableEmergencyLights()
        {
            Instance.IsSirenOn = true;
            Instance.IsSirenSilent = true;
        }

        public override string ToString()
        {
            return $"Position: {Instance.Position}, {nameof(IsBusy)}: {IsBusy}, {nameof(Driver)}: {Driver}, {nameof(Passengers)}: {Passengers?.Count}";
        }
    }
}