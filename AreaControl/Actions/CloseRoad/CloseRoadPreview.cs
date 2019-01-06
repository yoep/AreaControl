using AreaControl.AbstractionLayer;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadPreview : AbstractCloseRoad
    {
        public CloseRoadPreview(IRage rage)
            : base(rage)
        {
        }

        /// <inheritdoc />
        public override UIMenuItem Item { get; } = new UIMenuItem(AreaControl.ActionCloseRoadPreview);

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override void OnMenuActivation()
        {
            Rage.NewSafeFiber(() =>
            {
                var closestRoad = DetermineClosestRoad();
                var blockSlots = DetermineBlockSlots();
                closestRoad.CreatePreview();
                
                foreach (var slot in blockSlots)
                {
                    slot.CreatePreview();
                }
            }, "AreaControl.CloseRoadPreview");
        }
    }
}