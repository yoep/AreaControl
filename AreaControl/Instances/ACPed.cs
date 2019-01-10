using System.Diagnostics.CodeAnalysis;
using AreaControl.Duties;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines a <see cref="Ped"/> which is managed by the AreaControl plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACPed : IACEntity
    {
        private Entity _attachment;
        private IDuty _duty;

        public ACPed(Ped instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Get the GTA V Ped instance.
        /// </summary>
        public Ped Instance { get; }

        /// <summary>
        /// Get the current duty of the ped.
        /// </summary>
        public IDuty CurrentDuty => _duty;

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

        /// <summary>
        /// Attach the given entity to this ped.
        /// </summary>
        /// <param name="entity">Set the entity to attach.</param>
        public void Attach(Entity entity)
        {
            Assert.NotNull(entity, "entity cannot be null");
            _attachment = entity;

            EntityUtil.AttachEntity(entity, Instance, PlacementType.RightHand);
        }

        /// <summary>
        /// Walk to the given target entity (can be a ped or a vehicle).
        /// </summary>
        /// <param name="target">Set the target to walk to.</param>
        public TaskExecutor WalkTo(Entity target)
        {
            return TaskUtil.GoToEntity(Instance, target, 1f);
        }

        /// <summary>
        /// Look at the target entity.
        /// </summary>
        /// <param name="target">Set the target to look at.</param>
        /// <param name="duration">Set the duration to look at the ped.</param>
        /// <returns></returns>
        public TaskExecutor LookAt(Entity target, int duration = -1)
        {
            return TaskUtil.LookAtEntity(Instance, target, duration);
        }

        public TaskExecutor PlayAnimation()
        {
            
        }

        /// <summary>
        /// Activates the given duty for the ped.
        /// </summary>
        /// <param name="duty">Set the duty to activate</param>
        public void ActivateDuty(IDuty duty)
        {
            if (CurrentDuty != null && CurrentDuty.IsActive)
                throw new ActiveDutyException("Ped has currently an active duty running and cannot be reassigned to a new duty");

            _duty = duty;
            _duty.Execute(this);
        }

        /// <inheritdoc />
        public void Delete()
        {
            if (_attachment != null)
                _attachment.Delete();

            Instance.Delete();
        }
    }
}