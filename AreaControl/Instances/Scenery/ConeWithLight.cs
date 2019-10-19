using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public class ConeWithLight : AbstractPlaceableSceneryItem
    {
        public ConeWithLight(Vector3 position, float heading) : base(position, heading)
        {
        }

        protected override Object CreateItemInstance()
        {
            return PropUtils.CreateConeWithLight(Position, Heading);
        }
    }
}