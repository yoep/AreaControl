using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Instances.Exceptions;
using AreaControl.Utils.Tasks;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Duties
{
    /// <inheritdoc />
    /// <summary>
    /// Duty which lets the ped return to it's last vehicle.
    /// </summary>
    public class ReturnToVehicleDuty : AbstractOnPursuitAwareDuty
    {
        private TaskExecutor _currentTaskExecutor;

        #region Constructors

        internal ReturnToVehicleDuty(long id, ACPed ped)
            : base(id, ped)
        {
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        /// Return to vehicle can always be executed
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.ReturnToVehicle;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.All;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Logger.Info($"Executing return to vehicle duty #{Id}");
            Ped.WeaponsEnabled = true;

            try
            {
                var enterLastVehicleTask = _currentTaskExecutor = Ped.EnterLastVehicle(MovementSpeed.Walk);

                enterLastVehicleTask.OnAborted += (sender, args) => Ped.WarpIntoVehicle(Ped.LastVehicle, Ped.LastVehicleSeat);
                enterLastVehicleTask.WaitForCompletion(20000);
            }
            catch (VehicleNotAvailableException ex)
            {
                Logger.Error($"Return to vehicle duty could not be executed: {ex.Message}", ex);
                Ped.WanderAround();
            }
            Logger.Info($"Completed return to vehicle duty #{Id}");
        }

        #endregion

        #region AbstractOnPursuitAwareDuty

        protected override void OnPursuitStarted(LHandle pursuitHandle)
        {
            _currentTaskExecutor?.Abort();
            Functions.AddCopToPursuit(pursuitHandle, Ped.Instance);
            State = DutyState.Interrupted;
        }

        protected override void OnPursuitEnded(LHandle pursuitHandle)
        {
            Functions.RemovePedFromPursuit(Ped.Instance);
            Execute(); //resume duty
        }

        #endregion
    }
}