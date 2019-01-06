namespace AreaControl.Menu
{
    public interface IMenu
    {
        /// <summary>
        /// Check if the menu has been initialized.
        /// </summary>
        /// <returns>Returns true if menu is initialized and activated, else false.</returns>
        bool IsMenuInitialized { get; }

        void RegisterComponent(IMenuComponent component);
    }
}