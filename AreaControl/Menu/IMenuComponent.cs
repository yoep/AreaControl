using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public interface IMenuComponent
    {
        /// <summary>
        /// Get the menu item to register at nativeUI.
        /// </summary>
        UIMenuItem MenuItem { get; }
        
        /// <summary>
        /// Get if the menu component is automatically closed when selected in the menu.
        /// </summary>
        bool IsAutoClosed { get; }
        
        /// <summary>
        /// Get if the menu component is visible in the menu.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        /// <param name="sender">The menu that activated the menu item.</param>
        void OnMenuActivation(IMenu sender);

        /// <summary>
        /// Is triggered when the menu item is highlighted in the menu and the menu is open.
        /// </summary>
        /// <param name="sender">The menu that triggered this.</param>
        void OnMenuHighlighted(IMenu sender);
    }
}