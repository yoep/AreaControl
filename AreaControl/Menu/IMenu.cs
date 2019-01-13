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
        /// Get the total menu items that are available.
        /// </summary>
        int TotalItems { get; }

        /// <summary>
        /// Register the given component in the menu.
        /// </summary>
        /// <param name="component">Set the component to register.</param>
        void RegisterComponent(IMenuComponent component);
    }
}