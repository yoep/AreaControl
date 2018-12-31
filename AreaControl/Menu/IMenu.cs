using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public interface IMenu
    {
        /// <summary>
        /// Check if the menu has been initialized.
        /// </summary>
        /// <returns>Returns true if menu is initialized and activated, else false.</returns>
        bool IsMenuInitialized { get; }

        /// <summary>
        /// Register a new menu item with it's event handler to the AreaControl menu.
        /// </summary>
        /// <param name="item">Set the menu item.</param>
        /// <param name="component">Set the component which handles the menu item selected event.</param>
        void RegisterItem(UIMenuItem item, IMenuComponent component);
    }
}