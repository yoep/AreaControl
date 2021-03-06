using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public class Cone: AbstractPlaceableSceneryItem
    {
        public Cone(Vector3 position, float heading)
            : base(position, heading)
        {
        }

        #region Functions

        protected override Object CreateItemInstance()
        {
            return PropUtils.CreateSmallConeWithStripes(Position);
        }

        #endregion
    }
}