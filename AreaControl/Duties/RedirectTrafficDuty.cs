using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Duties
{
    /// <summary>
    /// Duty for redirecting the traffic on the road.
    /// This duty will let the given ped walk to the given position and play the parking assistant animation.
    /// </summary>
    public class RedirectTrafficDuty : IDuty
    {
        private readonly IRage _rage;
        private readonly Vector3 _position;
        private readonly float _heading;
        private ACPed _ped;
        private AnimationTaskExecutor _animationTaskExecutor;

        public RedirectTrafficDuty(Vector3 position, float heading)
        {
            _rage = IoC.Instance.GetInstance<IRage>();
            _position = position;
            _heading = heading;
        }

        /// <inheritdoc />
        public bool IsAvailable => true;

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
            _ped = ped;
            _rage.NewSafeFiber(() =>
            {
                var taskExecutor = ped.WalkTo(_position, _heading)
                    .WaitForCompletion(10000);
                //TODO: fix this completion
                _rage.LogTrivialDebug("Completed walk to redirect traffic position with " + taskExecutor);
                
                _rage.LogTrivialDebug("Attaching wand to ped...");
                ped.Attach(PropUtil.CreateWand());
                _rage.LogTrivialDebug("Starting to play redirect traffic animation...");
                _animationTaskExecutor = ped.PlayAnimation("amb@world_human_car_park_attendant@male@base", "base", AnimationFlags.Loop);
            }, typeof(RedirectTrafficDuty).Name);
        }

        /// <inheritdoc />
        public void Abort()
        {
            _animationTaskExecutor?.Abort();
            _ped.Instance.Tasks.EnterVehicle(_ped.Instance.LastVehicle, (int) VehicleSeat.Driver)
                .WaitForCompletion();
            Functions.SetPedAsCop(_ped.Instance);
        }
    }
}