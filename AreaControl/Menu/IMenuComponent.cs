using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public interface IMenuComponent
    {
        /// <summary>
        /// Get the menu item to register at nativeUI.
        /// </summary>
        UIMenuItem Item { get; }
        
        /// <summary>
        /// Get if the menu component is automatically closed when selected in the menu.
        /// </summary>
        bool IsAutoClosed { get; }

        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        void OnMenuActivation();
    }
}