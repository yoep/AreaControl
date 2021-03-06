using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadPreview : AbstractCloseRoad, ICloseRoadPreview
    {
        private IEnumerable<PoliceSlot> _blockSlots;

        public CloseRoadPreview(IRage rage, ILogger logger)
            : base(rage, logger)
        {
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionCloseRoadPreview);

        /// <inheritdoc />
        public override MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                RemovePreview();
            }
            else
            {
                CreatePreview();
            }
        }

        #endregion

        private void RemovePreview()
        {
            foreach (var blockSlot in _blockSlots)
            {
                blockSlot.DeletePreview();
            }
            _roads.ToList().ForEach(x => x.DeletePreview());

            MenuItem.Text = AreaControl.ActionCloseRoadPreview;
            IsActive = false;
        }

        private void CreatePreview()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.ActionRemoveCloseRoadPreview;
            Rage.NewSafeFiber(() =>
            {
                _blockSlots = DetermineBlockSlots();

                foreach (var slot in _blockSlots)
                {
                    slot.CreatePreview();
                }
                _roads.ToList().ForEach(x => x.CreatePreview());
            }, "AreaControl.CloseRoadPreview");
        }
    }
}