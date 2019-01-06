using System.Linq;
using Rage;

namespace AreaControl.Utils.Query
{
    public static class VehicleQuery
    {
        /// <summary>
        /// Find a vehicle within the given range of the given position.
        /// </summary>
        /// <param name="position">Set the position to search around.</param>
        /// <param name="radius">Set the radius to search in.</param>
        /// <returns>Returns a police vehicle in the range of the position if found, else false.</returns>
        public static Vehicle FindWithin(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius cannot be a negative number");
            
            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .FirstOrDefault(e => e.Driver != Game.LocalPlayer.Character && e.IsPoliceVehicle && !e.IsEmpty && e.IsDriveable);
        }
    }
}