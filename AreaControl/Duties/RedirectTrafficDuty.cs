using AreaControl.Menu;
using AreaControl.Menu.Response;
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
        private readonly Vector3 _position;
        private readonly float _heading;
        private readonly ResponseCode _code;
        private AnimationTaskExecutor _animationTaskExecutor;

        public RedirectTrafficDuty(Vector3 position, float heading, ResponseCode code)
        {
            _position = position;
            _heading = heading;
            _code = code;
        }

        #region IDuty

        /// <inheritdoc />
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        public override void Abort()
        {
            base.Abort();

            Rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = true;
                Rage.LogTrivialDebug("Aborting redirect animation...");
                _animationTaskExecutor?.Abort();
                Rage.LogTrivialDebug("Deleting attachments from redirect officer ped...");
                Ped.DeleteAttachments();
            }, "RedirectTrafficDuty.Abort");
        }

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Rage.NewSafeFiber(() =>
            {
                Ped.WeaponsEnabled = false;
                PlayRedirectTrafficAnimation();
            }, "RedirectTrafficDuty.Execute");
        }

        #endregion

        #region Functions

        private void PlayRedirectTrafficAnimation()
        {
            var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(_position, _heading) : Ped.RunTo(_position, _heading);
            var taskExecutor = goToExecutor
                .WaitForCompletion(20000);
            Rage.LogTrivialDebug("Completed walk to redirect traffic position with " + taskExecutor);
            Rage.LogTrivialDebug("Starting to play redirect traffic animation...");
            _animationTaskExecutor = AnimationUtils.RedirectTraffic(Ped);
            _animationTaskExecutor.OnCompletion += (sender, args) =>
            {
                if (Ped.IsValid)
                    PlayRedirectTrafficAnimation();
            };
        }

        #endregion
    }
}