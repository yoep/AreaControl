using System;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
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
        private readonly IRage _rage;
        private ACPed _ped;

        internal CleanCorpsesDuty(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        #region IDuty implementation

        /// <inheritdoc />
        public bool IsAvailable => IsDeadBodyInRange();

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsRepeatable => false;

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
                var deathPed = GetFirstAvailableDeathPed();

                ped.WalkTo(deathPed)
                    .WaitForAndExecute(taskExecutor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for walking to death ped " + taskExecutor);
                        return ped.LookAt(deathPed);
                    }, 30000)
                    .WaitForAndExecute(taskExecutor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for looking at ped " + taskExecutor);
                        Functions.CallCoroner(deathPed.Position, false);
                        _rage.LogTrivialDebug("Called coroner");

                        return AnimationUtil.TalkToRadio(ped);
                    }, 2000);

                AnimationUtil.IssueTicket(_ped)
                    .WaitForAndExecute(() => _ped.DeleteAttachments());

                while (IsDeadBodyInRange())
                {
                    GameFiber.Yield();
                }

                _rage.LogTrivialDebug("CleanCorpsesDuty has been completed");
                IsActive = false;
                EndDuty();
            }, "CleanCorpsesDuty");
        }

        /// <inheritdoc />
        public void Abort()
        {
            _rage.LogTrivialDebug("Ped is entering last vehicle for CleanCorpsesDuty");
            _ped.EnterLastVehicle(MovementSpeed.Walk)
                .WaitForAndExecute(() => _ped.ReturnToLspdfrDuty());
            _rage.LogTrivialDebug("Ped should have been returned to LSPDFR duty for CleanCorpsesDuty");
        }

        #endregion

        #region Functions

        private void EndDuty()
        {
            OnCompletion?.Invoke(this, EventArgs.Empty);
        }

        private bool IsDeadBodyInRange()
        {
            return PedQuery.FindWithin(_position, SearchRange).Any(x => x.IsDead);
        }

        private Ped GetFirstAvailableDeathPed()
        {
            return PedQuery.FindWithin(_position, SearchRange).FirstOrDefault(x => x.IsValid() && x.IsDead);
        }

        #endregion
    }
}