using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadPreview : AbstractCloseRoad, ICloseRoadPreview
    {
        private Road _road;
        private IEnumerable<BlockSlot> _blockSlots;

        public CloseRoadPreview(IRage rage)
            : base(rage)
        {
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionCloseRoadPreview);

        /// <inheritdoc />
        public override bool IsVisible => !IsActive;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            IsActive = true;
            sender.ReplaceComponent(this, new RemoveCloseRoadPreview(this));
            Rage.NewSafeFiber(() =>
            {
                _road = DetermineClosestRoad();
                _blockSlots = DetermineBlockSlots();
                _road.CreatePreview();

                foreach (var slot in _blockSlots)
                {
                    slot.CreatePreview();
                }
            }, "AreaControl.CloseRoadPreview");
        }

        #endregion

        #region ICloseRoad implementation

        /// <inheritdoc />
        public override void OpenRoad()
        {
            _road.DeletePreview();

            foreach (var blockSlot in _blockSlots)
            {
                blockSlot.DeletePreview();
            }

            IsActive = false;
        }

        #endregion
    }
}