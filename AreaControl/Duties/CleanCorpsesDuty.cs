using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils.Query;
using Rage;

namespace AreaControl.Duties
{
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

                ped.WalkTo(deathPed);
            }, "CleanCorpsesDuty");
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