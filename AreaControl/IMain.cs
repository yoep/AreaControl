namespace AreaControl
{
    /// <summary>
    /// Controls all components of this plugin and is responsible for loading and unloading of all components.
    /// </summary>
    public interface IMain
    {
        /// <summary>
        /// Unload all components of this plugin.
        /// </summary>
        void Unload();
    }
}