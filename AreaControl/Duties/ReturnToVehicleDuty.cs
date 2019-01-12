using System;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public class ReturnToVehicleDuty : IDuty
    {
        private readonly IRage _rage;

        public ReturnToVehicleDuty()
        {
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        /// Return to vehicle can always be executed
        public bool IsAvailable => true;

        /// <inheritdoc />
        public bool IsActive { get; private set; }
        
        /// <inheritdoc />
        public bool IsRepeatable => true;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            _rage.LogTrivialDebug("Executing ReturnToVehicleDuty...");
            IsActive = true;
            ped
                .EnterLastVehicle(MovementSpeed.Walk)
                .WaitForAndExecute(() =>
                {
                    _rage.LogTrivialDebug("ReturnToVehicleDuty completed");
                    ped.ReturnToLspdfrDuty();
                    IsActive = false;
                    OnCompletion?.Invoke(this, EventArgs.Empty);
                }, 30000);
        }

        /// <inheritdoc />
        public void Abort()
        {
            //this duty cannot be aborted
        }
    }
}