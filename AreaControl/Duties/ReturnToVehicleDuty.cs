using AreaControl.AbstractionLayer;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public class ReturnToVehicleDuty : AbstractDuty
    {
        private readonly IRage _rage;

        public ReturnToVehicleDuty()
        {
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        /// Return to vehicle can always be executed
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public override void Execute()
        {
            base.Execute();
            _rage.LogTrivialDebug("Executing ReturnToVehicleDuty...");

            _rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = true;
                Ped
                    .EnterLastVehicle(MovementSpeed.Walk)
                    .WaitForAndExecute(() =>
                    {
                        _rage.LogTrivialDebug("ReturnToVehicleDuty completed");
                        CompleteDuty();
                    }, 30000);
            }, "ReturnToVehicleDuty.Execute");
        }
    }
}