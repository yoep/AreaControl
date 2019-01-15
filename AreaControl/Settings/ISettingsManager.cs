namespace AreaControl.Settings
{
    public interface ISettingsManager
    {
        /// <summary>
        /// Get the general settings for this plugin.
        /// </summary>
        GeneralSettings GeneralSettings { get; }
        
        /// <summary>
        /// Get the redirect traffic settings for this plugin.
        /// </summary>
        RedirectTrafficSettings RedirectTrafficSettings { get; }
    }
}