using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    public class Barrier : AbstractPlaceableSceneryItem
    {
        public Barrier(Vector3 position, float heading)
            : base(position, heading)
        {
        }

        #region Functions

        /// <inheritdoc />
        protected override Object CreateItemInstance()
        {
            return PropUtils.CreatePoliceDoNotCrossBarrier(Position, Heading);
        }

        #endregion
    }
}