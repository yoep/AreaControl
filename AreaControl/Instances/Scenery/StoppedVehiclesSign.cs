using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public class StoppedVehiclesSign : SceneryItem
    {
        public StoppedVehiclesSign(Vector3 position, float heading)
            : base(position, heading)
        {
        }

        #region Functions

        protected override Object CreateItemInstance()
        {
            return PropUtils.StoppedVehiclesSign(Position, Heading);
        }

        #endregion
    }
}