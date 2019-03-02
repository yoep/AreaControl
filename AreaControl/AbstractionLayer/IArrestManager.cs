using Rage;

namespace AreaControl.AbstractionLayer
{
    public interface IArrestManager
    {
        /// <summary>
        /// Request a tow truck for the given vehicle.
        /// </summary>
        /// <param name="vehicle">Set the vehicle to tow.</param>
        /// <param name="radioAnimation">Set if the radio animation must be played for the player.</param>
        void RequestTowTruck(Vehicle vehicle, bool radioAnimation);
        
        /// <summary>
        /// Request a coroner for the given position.
        /// </summary>
        /// <param name="position">Set the destination of the coroner.</param>
        /// <param name="radioAnimation">Set if the radio animation must be played for the player.</param>
        void CallCoroner(Vector3 position, bool radioAnimation);
    }
}