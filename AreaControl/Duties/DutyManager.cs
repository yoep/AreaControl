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
        public IDuty NextAvailableOrIdleDuty(Vector3 position)
        {
            var nextAvailableDuty = GetNextAvailableDuty(position);

            if (nextAvailableDuty != null)
            {
                RegisterDuty(nextAvailableDuty);
                return nextAvailableDuty;
            }

            _rage.LogTrivialDebug("Their are no available duties in the area of " + position);
            return GetIdleDuty();
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

        private IDuty GetNextAvailableDuty(Vector3 position)
        {
            return GetDuties(position).FirstOrDefault(x => x.IsAvailable && IsInstantiationAllowed(x));
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
            };
        }

        private static IDuty GetIdleDuty()
        {
            return new ReturnToVehicleDuty();
        }
    }
}