using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Menu;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using Arrest_Manager.API;
using Rage;

namespace AreaControl.Duties
{
    /// <summary>
    /// Duty for cleaning dead bodies.
    /// This duty consist out of calling the EMS first. If one or more bodies couldn't be revived, they will call the coroner service.
    /// </summary>
    public class CleanCorpsesDuty : AbstractDuty
    {
        private const float SearchRange = 35f;

        private readonly Vector3 _position;
        private readonly ResponseCode _code;
        private readonly IRage _rage;

        internal CleanCorpsesDuty(Vector3 position, ResponseCode code)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            _code = code;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        #region IDuty implementation

        /// <inheritdoc />
        public override bool IsAvailable => IsDeadBodyInRange();

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public override void Execute()
        {
            base.Execute();

            _rage.NewSafeFiber(() =>
            {
                _rage.LogTrivialDebug("Executing CleanCorpsesDuty...");
                var deathPed = GetFirstAvailableDeathPed();
                var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(deathPed) : Ped.RunTo(deathPed);

                goToExecutor
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for walking to death ped " + executor);
                        return Ped.LookAt(deathPed);
                    }, 30000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for looking at ped " + executor);
                        return AnimationUtil.TalkToRadio(Ped);
                    }, 3000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor talking to radio " + executor);
                        _rage.LogTrivialDebug("Calling coroner...");
                        Functions.CallCoroner(deathPed.Position, false);
                        return AnimationUtil.Investigate(Ped);
                    }, 3000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed animation executor for investigate " + executor);
                        Ped.DeleteAttachments();

                        while (IsDeadBodyInRange())
                        {
                            GameFiber.Yield();
                        }

                        _rage.LogTrivialDebug("CleanCorpsesDuty has been completed");
                        CompleteDuty();
                    }, 5000);
            }, "CleanCorpsesDuty.Execute");
        }

        #endregion

        #region Functions

        private bool IsDeadBodyInRange()
        {
            return PedQuery.FindWithin(_position, SearchRange).Any(x => x.IsValid() && x.IsDead);
        }

        private Ped GetFirstAvailableDeathPed()
        {
            return PedQuery.FindWithin(_position, SearchRange).FirstOrDefault(x => x.IsValid() && x.IsDead);
        }

        #endregion
    }
}