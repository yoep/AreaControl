using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeScenePreview : AbstractCrimeScene, ICrimeScenePreview
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private CrimeSceneSlot _crimeSceneSlot;

        public CrimeScenePreview(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.CrimeScenePreview, AreaControl.CrimeSceneDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        public void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                DeletePreview();
            }
            else
            {
                CreatePreview();
            }
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => IsActive;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                _crimeSceneSlot = DetermineCrimeSceneSlot();
                _logger.Debug($"Created {_crimeSceneSlot.BarriersAndCones.Count} barriers for the crime scene");
                
                _crimeSceneSlot.CreatePreview();
                _crimeSceneSlot.StartPoint.CreatePreview();
                _crimeSceneSlot.EndPoint.CreatePreview();

                MenuItem.Text = AreaControl.CrimeScenePreviewRemove;
                IsActive = true;
            }, "CrimeScenePreview.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                _crimeSceneSlot.DeletePreview();
                _crimeSceneSlot.StartPoint.DeletePreview();
                _crimeSceneSlot.EndPoint.DeletePreview();
                _crimeSceneSlot = null;
                
                MenuItem.Text = AreaControl.CrimeScenePreview;
                IsActive = false;
            }, "CrimeScenePreview.DeletePreview");
        }

        #endregion
    }
}