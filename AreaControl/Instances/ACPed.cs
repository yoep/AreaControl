using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.Duties;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines a <see cref="Ped"/> which is managed by the AreaControl plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACPed : IACEntity
    {
        private readonly List<Entity> _attachments = new List<Entity>();
        private VehicleSeat _lastSeat;

        public ACPed(Ped instance, long id)
        {
            Instance = instance;
            Id = id;
        }

        /// <inheritdoc />
        public long Id { get; }

        /// <summary>
        /// Get the GTA V Ped instance.
        /// </summary>
        public Ped Instance { get; }

        /// <summary>
        /// Get the current duty of the ped.
        /// </summary>
        public IDuty CurrentDuty { get; private set; }

        /// <summary>
        /// Get or set if this vehicle is busy.
        /// If not busy, this vehicle can be used for a task by the plugin.
        /// </summary>
        public bool IsBusy { get; internal set; }

        /// <summary>
        /// Get the last vehicle of this ped.
        /// </summary>
        public ACVehicle LastVehicle { get; private set; }

        /// <summary>
        /// Warp this ped into a vehicle.
        /// </summary>
        /// <param name="vehicle">Set the vehicle to warp the ped into.</param>
        /// <param name="seat">Set the seat to place the ped in.</param>
        public void WarpIntoVehicle(ACVehicle vehicle, VehicleSeat seat)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");

            LastVehicle = vehicle;
            IsBusy = true;
            _lastSeat = seat;
            Instance.WarpIntoVehicle(vehicle.Instance, (int) seat);
        }

        /// <summary>
        /// Attach the given entity to this ped.
        /// </summary>
        /// <param name="attachment">Set the entity to attach.</param>
        /// <param name="placement">Set the attachment placement on the ped.</param>
        public void Attach(Entity attachment, PlacementType placement)
        {
            Assert.NotNull(attachment, "entity cannot be null");
            _attachments.Add(attachment);

            EntityUtil.AttachEntity(attachment, Instance, placement);
        }

        /// <summary>
        /// Walk to the given position.
        /// </summary>
        /// <param name="position">Set the position to go to.</param>
        /// <param name="heading">Set the heading when arriving.</param>
        /// <returns>Returns the task executor.</returns>
        public TaskExecutor WalkTo(Vector3 position, float heading)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.GoTo(Instance, position, heading, MovementSpeed.Walk);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Run to the given position.
        /// </summary>
        /// <param name="position">Set the position to go to.</param>
        /// <param name="heading">Set the heading when arriving.</param>
        /// <returns>Returns the task executor.</returns>
        public TaskExecutor RunTo(Vector3 position, float heading)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.GoTo(Instance, position, heading, MovementSpeed.Run);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Walk to the given target entity (can be a ped or a vehicle).
        /// </summary>
        /// <param name="target">Set the target to walk to.</param>
        public TaskExecutor WalkTo(Entity target)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.GoToEntity(Instance, target, MovementSpeed.Walk);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Look at the target entity.
        /// </summary>
        /// <param name="target">Set the target to look at.</param>
        /// <param name="duration">Set the duration to look at the ped.</param>
        /// <returns></returns>
        public TaskExecutor LookAt(Entity target, int duration = -1)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.LookAtEntity(Instance, target, duration);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Enter the last vehicle in the last seat.
        /// </summary>
        /// <param name="speed">Set the speed when going to the vehicle.</param>
        /// <returns>Returns the task executor for this task.</returns>
        public TaskExecutor EnterLastVehicle(MovementSpeed speed)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.EnterVehicle(Instance, LastVehicle.Instance, _lastSeat, speed);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Play the given animation on this ped.
        /// </summary>
        /// <param name="animationDictionary">Set the animation dictionary.</param>
        /// <param name="animationName">Set the animation name from within the dictionary to play.</param>
        /// <param name="animationFlags">Set the animation flags.</param>
        /// <returns>Returns the animation task executor.</returns>
        public AnimationTaskExecutor PlayAnimation(string animationDictionary, string animationName, AnimationFlags animationFlags)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.PlayAnimation(Instance, animationDictionary, animationName, animationFlags);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Activates the given duty for the ped.
        /// </summary>
        /// <param name="duty">Set the duty to activate</param>
        public void ActivateDuty(IDuty duty)
        {
            if (CurrentDuty != null && CurrentDuty.IsActive)
                throw new ActiveDutyException("Ped has currently an active duty running and cannot be reassigned to a new duty");

            CurrentDuty = duty;
            CurrentDuty.Execute(this);
        }

        /// <summary>
        /// Free this ped and return the instance handle back to LSPDFR.
        /// </summary>
        public void ReturnToLspdfrDuty()
        {
            IsBusy = false;
            DeleteAttachments();
            Functions.SetPedAsCop(Instance);
            Functions.SetCopAsBusy(Instance, false);
            Instance.Dismiss();
        }

        /// <inheritdoc />
        public void Delete()
        {
            DeleteAttachments();
            Instance.Delete();
        }

        /// <summary>
        /// Delete the attachments of this ped.
        /// </summary>
        public void DeleteAttachments()
        {
            foreach (var attachment in _attachments.Where(x => x.IsValid()))
            {
                EntityUtil.DetachEntity(attachment);
                attachment.Dismiss();
                attachment.Delete();
            }

            _attachments.Clear();
        }

        private EventHandler TaskExecutorOnCompletion()
        {
            return (sender, args) => IsBusy = false;
        }
    }
}