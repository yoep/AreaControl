using AreaControl.Settings;

namespace AreaControlTests.AbstractionLayer
{
    public class SettingsManagerImpl : ISettingsManager
    {
        public GeneralSettings GeneralSettings { get; }
        public CleanAreaSettings CleanAreaSettings { get; }
        public RedirectTrafficSettings RedirectTrafficSettings { get; }
        public CloseRoadSettings CloseRoadSettings { get; }
    }
}