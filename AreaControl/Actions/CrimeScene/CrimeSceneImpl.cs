using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeSceneImpl : AbstractCrimeScene, ICrimeScene
    {
        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.CrimeScene, AreaControl.CrimeSceneDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.AREA_CONTROL;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => true;

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