using AreaControl.AbstractionLayer;
using AreaControl.Debug;
using AreaControl.Instances;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectTrafficPreview : AbstractRedirectTraffic, IPreviewSupport
    {
        private readonly IRage _rage;
        private readonly IRoadPreview _roadPreview;
        private RedirectSlot _redirectSlot;

        public RedirectTrafficPreview(IRage rage, IRoadPreview roadPreview)
        {
            _rage = rage;
            _roadPreview = roadPreview;
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RedirectTrafficPreview);

        /// <inheritdoc />
        public override MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                IsActive = false;
                MenuItem.Text = AreaControl.RedirectTrafficPreview;
                _roadPreview.HideRoadPreview();
                DeletePreview();
            }
            else
            {
                IsActive = true;
                MenuItem.Text = AreaControl.RedirectTrafficPreviewRemove;
                _roadPreview.ShowRoadPreview();
                CreatePreview();
            }
        }

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive => IsActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                _redirectSlot = DetermineRedirectSlot();
                _redirectSlot.CreatePreview();
            }, "RedirectTrafficPreview.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                _redirectSlot.DeletePreview();
                _redirectSlot = null;
            }, "RedirectTrafficPreview.DeletePreview");
        }

        #endregion
    }
}