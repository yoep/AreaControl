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
        /// Get if the menu is a debug option.
        /// </summary>
        bool IsDebug { get; }

        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        /// <param name="sender">The menu that activated the menu item.</param>
        void OnMenuActivation(IMenu sender);
    }
}