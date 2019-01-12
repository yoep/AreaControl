using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using Rage;

namespace AreaControl.Duties
{
    public class DutyManager : IDutyManager
    {
        private readonly List<IDuty> _duties = new List<IDuty>();
        private readonly IRage _rage;

        #region Constructors

        public DutyManager(IRage rage)
        {
            _rage = rage;
        }

        #endregion

        /// <inheritdoc />
        public IDuty GetNextAvailableDuty(Vector3 position)
        {
            var nextAvailableDuty = GetDuties(position).FirstOrDefault(x => x.IsAvailable && IsInstantiationAllowed(x));

            if (nextAvailableDuty != null)
                _rage.LogTrivialDebug("Found next available duty " + nextAvailableDuty);
            else
                _rage.LogTrivialDebug("Their are no available duties in the area of " + position);

            _duties.Add(nextAvailableDuty);
            return nextAvailableDuty;
        }

        /// <inheritdoc />
        public void RegisterDuty(IDuty duty)
        {
            Assert.NotNull(duty, "duty cannot be null");
            _duties.Add(duty);
        }

        /// <inheritdoc />
        public void DismissDuties()
        {
            foreach (var duty in _duties.Where(x => x.IsActive))
            {
                duty.Abort();
            }
            _duties.Clear();
        }

        private bool IsInstantiationAllowed(IDuty duty)
        {
            if (duty.IsRepeatable)
                return true;

            return !HasAlreadyBeenInstantiatedBefore(duty);
        }

        private bool HasAlreadyBeenInstantiatedBefore(IDuty duty)
        {
            return _duties.Any(x => x.GetType() == duty.GetType());
        }

        private static IEnumerable<IDuty> GetDuties(Vector3 position)
        {
            return new List<IDuty>
            {
                new CleanCorpsesDuty(position),
                new CleanWrecksDuty(position),
                new ReturnToVehicleDuty()
            };
        }
    }
}