using Rage;

namespace AreaControl.Actions.Model
{
    public interface IVehicleSlot
    {
        /// <summary>
        /// Get the position of the vehicle.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the vehicle.
        /// </summary>
        float Heading { get; }
    }
}