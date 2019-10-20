using System.Diagnostics.CodeAnalysis;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
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

        public RedirectTrafficDuty(long id, ACPed ped, Vector3 position, float heading, ResponseCode code)
            : base(id, ped)
        {
            _position = position;
            _heading = heading;
            _code = code;
        }

        #region Properties

        /// <inheritdoc />
        public override bool IsAvailable => true;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.RedirectTraffic;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.Cops;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        public override void Abort()
        {
            Ped.WeaponsEnabled = true;
            Logger.Trace("Aborting redirect traffic animation");
            _animationTaskExecutor?.Abort();
            Logger.Trace("Deleting attachments from the redirect traffic officer ped");
            Ped.DeleteAttachments();

            base.Abort();
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        protected override void DoExecute()
        {
            Logger.Info($"Executing redirect traffic duty #{Id}");
            Ped.WeaponsEnabled = false;
            PlayRedirectTrafficAnimation();
            
            // keep this method alive so it doesn't finish
            // this duty is a continuous task, so it never finishes until it is aborted
            // this means that it never should reach the state "DutyState.Completed"
            while (true)
            {
                GameFiber.Yield();
            }
        }

        #endregion

        #region Functions

        private void PlayRedirectTrafficAnimation()
        {
            var goToTaskExecutor = CreateGoToTask().WaitForCompletion(10000);
            Logger.Trace("Completed walk to redirect traffic position with " + goToTaskExecutor);

            Logger.Trace("Starting to play redirect traffic animation...");
            _animationTaskExecutor = AnimationUtils.RedirectTraffic(Ped);
            _animationTaskExecutor.OnCompletion += (sender, args) =>
            {
                if (Ped.IsValid)
                    PlayRedirectTrafficAnimation();
            };
        }

        private TaskExecutor CreateGoToTask()
        {
            var goToExecutor = _code == ResponseCode.Code2
                ? Ped.WalkTo(_position, _heading)
                : Ped.RunTo(_position, _heading);

            goToExecutor.OnAborted += (sender, args) => OnGoToAborted();

            return goToExecutor;
        }

        private void OnGoToAborted()
        {
            // warp player into correct position as the go to task executor has been aborted
            Logger.Trace("Warping ped into position for RedirectTrafficDuty as the task executor has been aborted");
            Ped.WarpTo(_position, _heading);
        }

        #endregion
    }
}