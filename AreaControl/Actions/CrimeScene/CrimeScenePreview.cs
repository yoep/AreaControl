using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeScenePreview : AbstractCrimeScene, ICrimeScenePreview
    {
        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.CrimeScene, AreaControl.CrimeSceneDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        public void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                RemoveCrimeScene();
            }
            else
            {
                CreateCrimeScene();
            }
        }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive { get; }

        /// <inheritdoc />
        public void CreatePreview()
        {
           
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            
        }

        #endregion

        #region Functions

        private void RemoveCrimeScene()
        {
        }

        private void CreateCrimeScene()
        {
        }

        #endregion
    }
}