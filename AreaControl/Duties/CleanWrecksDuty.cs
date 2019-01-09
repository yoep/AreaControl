using AreaControl.Instances;

namespace AreaControl.Duties
{
    public class CleanWrecksDuty : IDuty
    {
        /// <inheritdoc />
        public bool IsAvailable { get; }
        
        /// <inheritdoc />
        public bool IsActive { get; private set; }
        
        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
        }
        
        /// <inheritdoc />
        public void Abort()
        {
            
        }
    }
}