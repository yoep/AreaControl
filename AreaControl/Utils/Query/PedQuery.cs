using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AreaControl.Utils.Query
{
    public static class PedQuery
    {
        /// <summary>
        /// Find human ped's around the given position.
        /// The player ped is excluded from this search.
        /// </summary>
        /// <param name="position">Set the position to search around.</param>
        /// <param name="radius">Set the radius to search within.</param>
        /// <returns>Returns a list of found ped's within the range.</returns>
        public static List<Ped> FindWithin(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius cannot be a negative number");
            
            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ExcludePlayerPed)
                .OfType<Ped>()
                .ToList();
        }
    }
}