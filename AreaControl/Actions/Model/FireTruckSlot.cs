using AreaControl.Utils;
using Rage;

namespace AreaControl.Actions.Model
{
    public class FireTruckSlot : AbstractVehicleSlot
    {
        public FireTruckSlot(Vector3 position, float heading)
            : base(position, heading)
        {
        }

        protected override Rage.Model GetModelInstance()
        {
            return ModelUtils.GetFireTruck();
        }
    }
}