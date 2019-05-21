using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AreaControl.Utils.Query
{
    public static class EntityQueryUtils
    {
        /// <summary>
        /// Find entities around the given position which are still alive.
        /// The player ped is included in this search.
        /// </summary>
        /// <param name="position">The position to search around.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <returns>Returns a list of found entities within the range.</returns>
        public static List<Entity> FindAliveEntitiesWithin(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius cannot be a negative number");
            
            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderHumanPeds | GetEntitiesFlags.ConsiderGroundVehicles)
                .Where(x => x.IsValid() && x.IsAlive)
                .ToList();
        }    
    }
}