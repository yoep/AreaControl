using System;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
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
    public class CleanCorpsesDuty : IDuty
    {
        private const float SearchRange = 35f;

        private readonly Vector3 _position;
        private readonly ResponseCode _code;
        private readonly IRage _rage;
        private ACPed _ped;

        internal CleanCorpsesDuty(Vector3 position, ResponseCode code)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            _code = code;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        #region IDuty implementation

        /// <inheritdoc />
        public bool IsAvailable => IsDeadBodyInRange();

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsRepeatable => true;

        /// <inheritdoc />
        public bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            if (!IsAvailable)
                return;

            IsActive = true;
            _ped = ped;
            _rage.NewSafeFiber(() =>
            {
                _rage.LogTrivialDebug("Executing CleanCorpsesDuty...");
                var deathPed = GetFirstAvailableDeathPed();
                var goToExecutor = _code == ResponseCode.Code2 ? ped.WalkTo(deathPed) : ped.RunTo(deathPed);
                
                goToExecutor
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for walking to death ped " + executor);
                        return ped.LookAt(deathPed);
                    }, 30000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for looking at ped " + executor);
                        return AnimationUtil.TalkToRadio(ped);
                    }, 3000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor talking to radio " + executor);
                        _rage.LogTrivialDebug("Calling coroner...");
                        Functions.CallCoroner(deathPed.Position, false);
                        return AnimationUtil.Investigate(_ped);
                    }, 3000)
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed animation executor for investigate " + executor);
                        _ped.DeleteAttachments();

                        while (IsDeadBodyInRange())
                        {
                            GameFiber.Yield();
                        }

                        _rage.LogTrivialDebug("CleanCorpsesDuty has been completed");
                        IsActive = false;
                        EndDuty();
                    }, 5000);
            }, "CleanCorpsesDuty.Execute");
        }

        /// <inheritdoc />
        public void Abort()
        {
            if (!IsActive)
                return;

            _rage.LogTrivialDebug("Aborting CleanCorpsesDuty");
            _ped.ActivateDuty(new ReturnToVehicleDuty());
        }

        #endregion

        #region Functions

        private void EndDuty()
        {
            OnCompletion?.Invoke(this, EventArgs.Empty);
        }

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