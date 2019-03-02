using Arrest_Manager.API;
using Rage;

namespace AreaControl.AbstractionLayer.Implementation
{
    public class ArrestManagerImpl : IArrestManager
    {
        /// <inheritdoc />
        public void RequestTowTruck(Vehicle vehicle, bool radioAnimation)
        {
            Functions.RequestTowTruck(vehicle, radioAnimation);
        }

        /// <inheritdoc />
        public void CallCoroner(Vector3 position, bool radioAnimation)
        {
            Functions.CallCoroner(position, radioAnimation);
        }
    }
}