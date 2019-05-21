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
        /// Get the type of the menu item.
        /// </summary>
        MenuType Type { get; }

        /// <summary>
        /// Get if the menu component is automatically closed when selected in the menu.
        /// </summary>
        bool IsAutoClosed { get; }

        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        /// <param name="sender">The menu that activated the menu item.</param>
        void OnMenuActivation(IMenu sender);
    }
}