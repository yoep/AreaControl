using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using Rage;

namespace AreaControl.Managers
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
            _rage.LogTrivialDebug("Found next available duty " + nextAvailableDuty);
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