using System;
using AreaControl.AbstractionLayer;
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
        private readonly IRage _rage;
        private TaskExecutor _currentTaskExecutor;

        public ReturnToVehicleDuty()
        {
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        #region Properties

        /// <inheritdoc />
        /// Return to vehicle can always be executed
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Execute()
        {
            base.Execute();
            _rage.LogTrivialDebug("Executing ReturnToVehicleDuty...");

            _rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = true;

                try
                {
                    var enterLastVehicleTask = _currentTaskExecutor = Ped.EnterLastVehicle(MovementSpeed.Walk);

                    enterLastVehicleTask.OnAborted += (sender, args) => Ped.WarpIntoVehicle(Ped.LastVehicle, Ped.LastVehicleSeat);
                    enterLastVehicleTask.WaitForAndExecute(() =>
                    {
                        _rage.LogTrivialDebug("ReturnToVehicleDuty completed");
                        CompleteDuty();
                    }, 30000);
                }
                catch (VehicleNotAvailableException ex)
                {
                    _rage.LogTrivial("ReturnToVehicleDuty could not be executed: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    Ped.WanderAround();
                    CompleteDuty();
                }
            }, "ReturnToVehicleDuty.Execute");
        }

        #endregion

        #region AbstractOnPursuitAwareDuty

        protected override void OnPursuitStarted(LHandle pursuitHandle)
        {
            _currentTaskExecutor.Abort();
            Functions.AddCopToPursuit(pursuitHandle, Ped.Instance);
        }

        protected override void OnPursuitEnded(LHandle pursuitHandle)
        {
            Functions.RemovePedFromPursuit(Ped.Instance);
            Execute(); //resume duty
        }

        #endregion
    }
}