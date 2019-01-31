using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;

namespace AreaControl.Instances
{
    /// <summary>
    /// Defines a <see cref="Ped"/> which is managed by the AreaControl plugin.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ACPed : AbstractACInstance<Ped>
    {
        private readonly List<Entity> _attachments = new List<Entity>();
        private bool _weaponsEnabled;

        #region Constructors

        public ACPed(Ped instance, long id)
            : base(instance, id)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or set if this vehicle is busy.
        /// If not busy, this vehicle can be used for a task by the plugin.
        /// </summary>
        public bool IsBusy { get; internal set; }

        /// <summary>
        /// Get or set if weapons are enabled on this ped.
        /// </summary>
        public bool WeaponsEnabled
        {
            get { return _weaponsEnabled; }
            set { SetWeaponState(value); }
        }

        /// <summary>
        /// Get the last vehicle of this ped.
        /// </summary>
        public ACVehicle LastVehicle { get; private set; }

        /// <summary>
        /// Get the last vehicle seat of this ped.
        /// </summary>
        public VehicleSeat LastVehicleSeat { get; private set; }

        #endregion

        #region Methods

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
            LastVehicleSeat = seat;
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
        /// Run to the given target.
        /// </summary>
        /// <param name="target">Set the target to walk to.</param>
        /// <returns>Returns the task executor.</returns>
        public TaskExecutor RunTo(Entity target)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.GoToEntity(Instance, target, MovementSpeed.Run);
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
            var taskExecutor = TaskUtil.EnterVehicle(Instance, LastVehicle.Instance, LastVehicleSeat, speed);
            taskExecutor.OnCompletion += TaskExecutorOnCompletion();
            return taskExecutor;
        }

        /// <summary>
        /// Leave the current vehicle.
        /// </summary>
        /// <param name="leaveVehicleFlags">Set the task flags.</param>
        /// <returns>Returns the task executor for this task.</returns>
        public TaskExecutor LeaveVehicle(LeaveVehicleFlags leaveVehicleFlags)
        {
            IsBusy = true;
            var taskExecutor = TaskUtil.LeaveVehicle(Instance, Instance.LastVehicle, leaveVehicleFlags);
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
        /// Free this ped and return the instance handle back to LSPDFR.
        /// </summary>
        public void ReturnToLspdfrDuty()
        {
            if (IsInvalid)
                return;

            IsBusy = false;
            DeleteAttachments();
            Functions.SetPedAsCop(Instance);
            Functions.SetCopAsBusy(Instance, false);
            Instance.Dismiss();
        }

        /// <inheritdoc />
        public override void Delete()
        {
            DeleteAttachments();
            base.Delete();
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

        /// <summary>
        /// Detach all attachments from this ped.
        /// </summary>
        public void DetachAttachments()
        {
            foreach (var attachment in _attachments.Where(x => x.IsValid()))
            {
                EntityUtil.DetachEntity(attachment);
            }

            _attachments.Clear();
        }

        public void CruiseWithVehicle()
        {
            Instance.Tasks.CruiseWithVehicle(30f);
        }

        #endregion

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}," + Environment.NewLine +
                   $"{nameof(Instance)}: {Instance}," + Environment.NewLine +
                   $"{nameof(IsBusy)}: {IsBusy}," + Environment.NewLine +
                   $"{nameof(WeaponsEnabled)}: {WeaponsEnabled}";
        }

        private EventHandler TaskExecutorOnCompletion()
        {
            return (sender, args) => IsBusy = false;
        }

        private void SetWeaponState(bool weaponsEnabled)
        {
            _weaponsEnabled = weaponsEnabled;
            NativeFunction.Natives.SET_PED_CAN_SWITCH_WEAPON(Instance, weaponsEnabled);
        }
    }
}