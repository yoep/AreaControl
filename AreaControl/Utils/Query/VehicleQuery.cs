using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AreaControl.Utils.Query
{
    public static class VehicleQuery
    {
        /// <summary>
        /// Find police vehicles within the given range of the given position.
        /// </summary>
        /// <param name="position">Set the position to search around.</param>
        /// <param name="radius">Set the radius to search in.</param>
        /// <returns>Returns one or more police vehicles in the range of the position if found, else false.</returns>
        public static List<Vehicle> FindPoliceVehiclesWithin(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius cannot be a negative number");

            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .Where(e => e.Driver != Game.LocalPlayer.Character && e.IsPoliceVehicle && !e.IsEmpty && e.IsDriveable)
                .ToList();
        }

        /// <summary>
        /// Find vehicles within the given range of the given position.
        /// </summary>
        /// <param name="position">Set the position to search around.</param>
        /// <param name="radius">Set the radius to search in.</param>
        /// <returns>Returns vehicles within the given range of the given position if found, else an empty list.</returns>
        public static IEnumerable<Vehicle> FindVehiclesWithin(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius cannot be a negative number");

            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .Where(x => x.IsValid())
                .ToList();
        }
    }
}