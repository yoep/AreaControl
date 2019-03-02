using Rage;

namespace AreaControl.AbstractionLayer.NoOp
{
    /// <summary>
    /// No-op implementation of <see cref="IArrestManager"/>
    /// </summary>
    public class ArrestManagerNoOp : IArrestManager
    {
        public void RequestTowTruck(Vehicle vehicle, bool playAnimation)
        {
            //no-op
        }

        public void CallCoroner(Vector3 position, bool radioAnimation)
        {
            //no-op
        }
    }
}