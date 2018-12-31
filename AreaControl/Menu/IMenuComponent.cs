namespace AreaControl.Menu
{
    public interface IMenuComponent
    {
        /// <summary>
        /// Is triggered when the component menu item has been selected.
        /// </summary>
        void OnMenuActivation();
    }
}