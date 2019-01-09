using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
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

        public CleanCorpsesDuty(Vector3 position)
        {
            Assert.NotNull(position, "position cannot be null");
            _position = position;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        public bool IsAvailable => CheckAvailability();

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            if (!IsAvailable)
                return;

            IsActive = true;
            _rage.NewSafeFiber(() =>
            {
                var deathPed = GetFirstAvailableDeathPed();

                ped.WalkTo(deathPed)
                    .WaitForAndExecute(taskExecutor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for walking to death ped " + taskExecutor);
                        return ped.LookAt(deathPed);
                    })
                    .WaitForAndExecute(taskExecutor =>
                    {
                        _rage.LogTrivialDebug("Completed task executor for looking at ped " + taskExecutor);
                        Functions.CallCoroner(deathPed.Position, false);
                        _rage.LogTrivialDebug("Called coroner");
                    }, 3000);

                while (deathPed.IsValid())
                {
                    GameFiber.Yield();
                }

                _rage.LogTrivialDebug("CleanCorpsesDuty has been completed");
                IsActive = false;
            }, "CleanCorpsesDuty");
        }

        /// <inheritdoc />
        public void Abort()
        {
            
        }

        private bool CheckAvailability()
        {
            return PedQuery.FindWithin(_position, SearchRange).Any(x => x.IsDead);
        }

        private Ped GetFirstAvailableDeathPed()
        {
            return PedQuery.FindWithin(_position, SearchRange).FirstOrDefault(x => x.IsDead);
        }
    }
}