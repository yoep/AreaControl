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
        public override UIMenuItem Item { get; } = new UIMenuItem("CloseRoad_PREVIEW_Placeholder");

        /// <inheritdoc />
        public override void OnMenuActivation()
        {
            Rage.NewSafeFiber(() =>
            {
                var blockSlots = DetermineBlockSlots();
                foreach (var slot in blockSlots)
                {
                    slot.CreatePreview();
                }
            }, "AreaControl.CloseRoadPreview");
        }
    }
}