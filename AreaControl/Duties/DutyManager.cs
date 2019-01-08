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
            var nextAvailableDuty = GetDuties(position).FirstOrDefault(x => x.IsAvailable);

            if (nextAvailableDuty != null)
                _rage.LogTrivialDebug("Found next available duty " + nextAvailableDuty);
            else
                _rage.LogTrivialDebug("Their are no available duties in the area of " + position);

            return nextAvailableDuty;
        }

        private IEnumerable<IDuty> GetDuties(Vector3 position)
        {
            return new List<IDuty>
            {
                new CleanCorpsesDuty(position)
            };
        }
    }
}