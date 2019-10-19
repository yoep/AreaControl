using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Menu.Response;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using Rage;

namespace AreaControl.Duties
{
    /// <summary>
    /// Duty for cleaning dead bodies.
    /// This duty consist out of calling the EMS first. If one or more bodies couldn't be revived, they will call the coroner service.
    /// </summary>
    public class CleanCorpsesDuty : AbstractDuty
    {
        private const float SearchRange = 50f;

        private readonly Vector3 _position;
        private readonly ResponseCode _code;
        private readonly IArrestManager _arrestManager;
        private readonly float _scanRadius;

        #region Constructors

        internal CleanCorpsesDuty(long id, ACPed ped)
            : base(id, ped)
        {
            _position = Ped.Instance.Position;
            _scanRadius = SearchRange;
            _arrestManager = IoC.Instance.GetInstance<IArrestManager>();
            _code = IoC.Instance.GetInstance<IResponseManager>().ResponseCode;
        }

        internal CleanCorpsesDuty(long id, ACPed ped, float scanRadius)
            : base(id, ped)
        {
            _position = Ped.Instance.Position;
            _scanRadius = scanRadius;
            _arrestManager = IoC.Instance.GetInstance<IArrestManager>();
            _code = IoC.Instance.GetInstance<IResponseManager>().ResponseCode;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool IsAvailable => IsDeadBodyInRange();

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.CleanCorpses;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.AllEmergency;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Logger.Info($"Executing clean corpses duty #{Id}");
            var deathPed = GetFirstAvailableDeathPed();
            var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(deathPed) : Ped.RunTo(deathPed);

            var investigateExecutor = goToExecutor
                .WaitForAndExecute(executor =>
                {
                    Rage.LogTrivialDebug("Completed task executor for walking to death ped " + executor);
                    return Ped.LookAt(deathPed);
                }, 30000)
                .WaitForAndExecute(executor =>
                {
                    Rage.LogTrivialDebug("Completed task executor for looking at ped " + executor);
                    return AnimationUtils.TalkToRadio(Ped);
                }, 3000)
                .WaitForAndExecute(executor =>
                {
                    Rage.LogTrivialDebug("Completed task executor talking to radio " + executor);

                    Rage.LogTrivialDebug("Calling coroner...");
                    _arrestManager.CallCoroner(deathPed.Position, false);

                    return AnimationUtils.Investigate(Ped);
                }, 10000)
                .WaitForCompletion(5000);

            Logger.Debug("Completed animation executor for investigate " + investigateExecutor);
            Ped.DeleteAttachments();

            while (IsDeadBodyInRange())
            {
                GameFiber.Yield();
            }
            Logger.Info($"Completed clean corpses duty #{Id}");
        }

        #endregion

        #region Functions

        private bool IsDeadBodyInRange()
        {
            return PedQueryUtils.FindWithin(_position, _scanRadius).Any(x => x.IsValid() && x.IsDead);
        }

        private Ped GetFirstAvailableDeathPed()
        {
            return PedQueryUtils.FindWithin(_position, _scanRadius).FirstOrDefault(x => x.IsValid() && x.IsDead);
        }

        #endregion
    }
}