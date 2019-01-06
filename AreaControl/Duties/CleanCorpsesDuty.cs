using AreaControl.AbstractionLayer;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public class CleanCorpsesDuty : IDuty
    {
        private readonly IRage _rage;

        public CleanCorpsesDuty()
        {
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        public bool IsActive { get; private set; }
        
        public void Execute(ACPed ped)
        {
            IsActive = true;
            
        }
    }
}