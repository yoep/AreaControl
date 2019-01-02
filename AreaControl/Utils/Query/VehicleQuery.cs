using System.Linq;
using Rage;

namespace AreaControl.Utils.Query
{
    public class VehicleQuery : IVehicleQuery
    {
        /// <inheritdoc />
        public Vehicle FindWithin(Vector3 position, float radius)
        {
            return World
                .GetEntities(position, radius, GetEntitiesFlags.ConsiderGroundVehicles)
                .OfType<Vehicle>()
                .FirstOrDefault(e => e.IsPoliceVehicle && !e.IsEmpty && e.IsDriveable);
        }
    }
}