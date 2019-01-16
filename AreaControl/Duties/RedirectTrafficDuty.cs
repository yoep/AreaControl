using System;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
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
        private readonly ResponseCode _code;
        private ACPed _ped;
        private AnimationTaskExecutor _animationTaskExecutor;

        public RedirectTrafficDuty(Vector3 position, float heading, ResponseCode code)
        {
            _rage = IoC.Instance.GetInstance<IRage>();
            _position = position;
            _heading = heading;
            _code = code;
        }

        /// <inheritdoc />
        public bool IsAvailable => true;

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsRepeatable => true;

        /// <inheritdoc />
        public bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
            _ped = ped;
            _rage.NewSafeFiber(() =>
            {
                _ped.WeaponsEnabled = false;
                var goToExecutor = _code == ResponseCode.Code2 ? _ped.WalkTo(_position, _heading) : _ped.RunTo(_position, _heading);
                var taskExecutor = goToExecutor
                    .WaitForCompletion(20000);
                _rage.LogTrivialDebug("Completed walk to redirect traffic position with " + taskExecutor);

                _rage.LogTrivialDebug("Starting to play redirect traffic animation...");
                _animationTaskExecutor = AnimationUtil.RedirectTraffic(_ped);
            }, "RedirectTrafficDuty.Execute");
        }

        /// <inheritdoc />
        public void Abort()
        {
            _rage.NewSafeFiber(() =>
            {
                _ped.WeaponsEnabled = true;
                _rage.LogTrivialDebug("Aborting redirect animation...");
                _animationTaskExecutor?.Abort();
                _rage.LogTrivialDebug("Deleting attachments from redirect officer ped...");
                _ped.DeleteAttachments();
                IsActive = false;
                _ped.ActivateDuty(new ReturnToVehicleDuty());
            }, "RedirectTrafficDuty.Abort");
        }
    }
}