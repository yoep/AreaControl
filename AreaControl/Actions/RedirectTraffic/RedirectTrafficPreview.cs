using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectTrafficPreview : AbstractRedirectTraffic, IPreviewSupport
    {
        private readonly IRage _rage;
        private RedirectSlot _redirectSlot;

        public RedirectTrafficPreview(IRage rage)
        {
            _rage = rage;
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RedirectTrafficPreview);

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override bool IsDebug => true;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                IsActive = false;
                MenuItem.Text = AreaControl.RedirectTrafficPreview;
                DeletePreview();
            }
            else
            {
                IsActive = true;
                MenuItem.Text = AreaControl.RedirectTrafficPreviewRemove;
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