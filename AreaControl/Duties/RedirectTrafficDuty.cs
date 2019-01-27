using AreaControl.AbstractionLayer;
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
    public class RedirectTrafficDuty : AbstractDuty
    {
        private readonly IRage _rage;
        private readonly Vector3 _position;
        private readonly float _heading;
        private readonly ResponseCode _code;
        private AnimationTaskExecutor _animationTaskExecutor;

        public RedirectTrafficDuty(Vector3 position, float heading, ResponseCode code)
        {
            _rage = IoC.Instance.GetInstance<IRage>();
            _position = position;
            _heading = heading;
            _code = code;
        }

        /// <inheritdoc />
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public override void Execute()
        {
            base.Execute();

            _rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = false;
                PlayRedirectTrafficAnimation();
            }, "RedirectTrafficDuty.Execute");
        }

        /// <inheritdoc />
        public override void Abort()
        {
            base.Abort();

            _rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = true;
                _rage.LogTrivialDebug("Aborting redirect animation...");
                _animationTaskExecutor?.Abort();
                _rage.LogTrivialDebug("Deleting attachments from redirect officer ped...");
                Ped.DeleteAttachments();
            }, "RedirectTrafficDuty.Abort");
        }

        private void PlayRedirectTrafficAnimation()
        {
            var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(_position, _heading) : Ped.RunTo(_position, _heading);
            var taskExecutor = goToExecutor
                .WaitForCompletion(20000);
            _rage.LogTrivialDebug("Completed walk to redirect traffic position with " + taskExecutor);
            _rage.LogTrivialDebug("Starting to play redirect traffic animation...");
            _animationTaskExecutor = AnimationUtil.RedirectTraffic(Ped);
            _animationTaskExecutor.OnCompletion += (sender, args) =>
            {
                if (Ped.IsValid)
                    PlayRedirectTrafficAnimation();
            };
        }
    }
}