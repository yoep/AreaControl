using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines a <see cref="Vehicle"/> which is managed by the AreaControl plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACVehicle : AbstractACInstance<Vehicle>
    {
        #region Constructors

        public ACVehicle(Vehicle instance, VehicleType type, long id)
            : base(instance, id, 1f)
        {
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the type of the vehicle.
        /// </summary>
        public VehicleType Type { get; }

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
        public bool IsBusy => Driver == null || Driver.IsBusy;

        /// <summary>
        /// Check if all original occupants or present again in this vehicle.
        /// </summary>
        public bool AllOccupantsPresent => Occupants.All(x => Instance.IsValid() && Instance.Occupants.Contains(x.Instance));

        /// <summary>
        /// Get if this vehicle is wandering around.
        /// </summary>
        public bool IsWandering { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Drive to the given position.
        /// </summary>
        /// <param name="position">Set the target position.</param>
        /// <param name="speed">Set the driving speed.</param>
        /// <param name="drivingFlags">Set the driving style.</param>
        /// <param name="acceptedDistance">Set the accepted distance from the target position.</param>
        /// <returns>Returns the task executor.</returns>
        public TaskExecutor DriveToPosition(Vector3 position, float speed, VehicleDrivingFlags drivingFlags, float acceptedDistance)
        {
            ChangeStateToBusy();
            var executor = TaskUtils.DriveToPosition(Driver.Instance, Instance, position, speed, drivingFlags, acceptedDistance);
            executor.OnCompletion += (sender, args) => Occupants.ForEach(x => x.IsBusy = false);
            return executor;
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

        /// <summary>
        /// Enable the hazard lights on this vehicle.
        /// </summary>
        public void EnableHazardIndicators()
        {
            if (Instance.IsValid())
                Instance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both;
        }

        /// <summary>
        /// Disables the emergency lights with sound.
        /// </summary>
        public void DisableSirens()
        {
            Instance.IsSirenOn = false;
        }

        /// <summary>
        /// Disable the hazard lights on this vehicle.
        /// </summary>
        public void DisableHazardLights()
        {
            if (Instance.IsValid())
                Instance.IndicatorLightsStatus = VehicleIndicatorLightsStatus.Off;
        }

        /// <summary>
        /// Let this vehicle wander around.
        /// </summary>
        public void Wander()
        {
            if (Driver == null || !Driver.IsValid || Instance.Driver != Driver.Instance)
                return;

            DeleteBlip();
            IsWandering = true;
            Occupants.ForEach(x =>
            {
                x.DeleteBlip();
                x.IsBusy = false;
            });

            // check if all occupants are present in the vehicle
            if (!AllOccupantsPresent)
            {
                TaskExecutor lastTask = null;

                Occupants.ForEach(x => lastTask = x.EnterLastVehicle(MovementSpeed.Walk));

                lastTask?.WaitForCompletion();
            }

            Driver.CruiseWithVehicle();
        }

        /// <summary>
        /// Set the busy state of all occupants.
        /// </summary>
        /// <param name="busy">Set the busy state for the occupants.</param>
        public void SetOccupantsBusyState(bool busy)
        {
            Occupants.ForEach(x => x.IsBusy = busy);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}," + Environment.NewLine +
                   $"Position: {Instance.Position}," + Environment.NewLine +
                   $"{nameof(IsBusy)}: {IsBusy}," + Environment.NewLine +
                   $"{nameof(Occupants)}: {Occupants?.Count}" + Environment.NewLine +
                   FormatDriver();
        }

        #endregion

        #region Functions

        private void ChangeStateToBusy()
        {
            IsWandering = false;
            Occupants.ForEach(x => x.IsBusy = true);
        }

        private string FormatDriver()
        {
            if (Driver == null)
                return $"{nameof(Driver)}: NULL";

            return $"--- {nameof(Driver)} ---" + Environment.NewLine +
                   Driver + Environment.NewLine +
                   "---";
        }

        #endregion
    }
}