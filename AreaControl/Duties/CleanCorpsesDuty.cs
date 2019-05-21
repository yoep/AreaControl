using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Menu;
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

        internal CleanCorpsesDuty(long id, Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            Id = id;
            _position = position;
            _scanRadius = SearchRange;
            _arrestManager = IoC.Instance.GetInstance<IArrestManager>();
            _code = IoC.Instance.GetInstance<IResponseManager>().ResponseCode;
        }

        internal CleanCorpsesDuty(long id, Vector3 position, float scanRadius)
        {
            Assert.NotNull(position, "position cannot be null");
            Id = id;
            _position = position;
            _scanRadius = scanRadius;
            _arrestManager = IoC.Instance.GetInstance<IArrestManager>();
            _code = IoC.Instance.GetInstance<IResponseManager>().ResponseCode;
        }

        #endregion

        #region IDuty

        /// <inheritdoc />
        public override bool IsAvailable => IsDeadBodyInRange();

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Rage.NewSafeFiber(() =>
            {
                Rage.LogTrivialDebug("Executing CleanCorpsesDuty...");
                var deathPed = GetFirstAvailableDeathPed();
                var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(deathPed) : Ped.RunTo(deathPed);

                goToExecutor
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
                    .WaitForAndExecute(executor =>
                    {
                        Rage.LogTrivialDebug("Completed animation executor for investigate " + executor);
                        Ped.DeleteAttachments();

                        while (IsDeadBodyInRange())
                        {
                            GameFiber.Yield();
                        }

                        Rage.LogTrivialDebug("CleanCorpsesDuty has been completed");
                        CompleteDuty();
                    }, 5000);
            }, "CleanCorpsesDuty.Execute");
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