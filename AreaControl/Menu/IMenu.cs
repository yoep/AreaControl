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

        /// <summary>
        /// Replaces the original component (which is registered), with the given new component in the menu.
        /// Will only be executed if the original component has been registered.
        /// </summary>
        /// <param name="originalComponent">Set the component from the menu that must be replaced.</param>
        /// <param name="newComponent">Set the new menu component.</param>
        void ReplaceComponent(IMenuComponent originalComponent, IMenuComponent newComponent);
    }
}