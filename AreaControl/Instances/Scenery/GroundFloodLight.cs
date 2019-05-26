using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public class GroundFloodLight : AbstractPlaceableSceneryItem
    {
        public GroundFloodLight(Vector3 position, float heading) 
            : base(position, heading)
        {
        }

        protected override Object CreateItemInstance()
        {
            return PropUtils.CreateGroundFloodLight(Position, Heading);
        }
    }
}