using AreaControl.Instances;

namespace AreaControl.Duties
{
    public class CleanWrecksDuty : IDuty
    {
        public bool IsActive { get; private set; }
        
        public void Execute(ACPed ped)
        {
            IsActive = true;
        }
    }
}