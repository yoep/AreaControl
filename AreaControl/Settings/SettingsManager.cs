using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using AreaControl.AbstractionLayer;
using Rage;

namespace AreaControl.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private const string File = @"./Plugins/LSPDFR/AreaControl.ini";
        private const string CleanAreaGroupName = "Clean Area";
        private const string RedirectTrafficGroupName = "Redirect Traffic";
        private const string CloseRoadGroupName = "Close Road";

        private readonly IRage _rage;

        public SettingsManager(IRage rage)
        {
            _rage = rage;
        }

        /// <inheritdoc />
        public GeneralSettings GeneralSettings { get; private set; }

        /// <inheritdoc />
        public CleanAreaSettings CleanAreaSettings { get; private set; }
        
        /// <inheritdoc />
        public RedirectTrafficSettings RedirectTrafficSettings { get; private set; }

        /// <inheritdoc />
        public CloseRoadSettings CloseRoadSettings { get; private set; }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void LoadSettings()
        {
            try
            {
                _rage.LogTrivial(System.IO.File.Exists(File) ? "Loading configuration file" : "Configuration file not found, using defaults instead");

                var settingsFile = new InitializationFile(File);

                ReadGeneralSettings(settingsFile);
                ReadCleanAreaSettings(settingsFile);
                ReadCloseRoadSettings(settingsFile);
                ReadRedirectTrafficSettings(settingsFile);
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("Failed to load settings with " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void ReadGeneralSettings(InitializationFile file)
        {
            GeneralSettings = new GeneralSettings
            {
                OpenMenuKey = ValueToKey(file.ReadString("General", "OpenMenuKey", "T")),
                OpenMenuModifierKey = ValueToKey(file.ReadString("General", "OpenMenuModifierKey", "Shift"))
            };
        }

        private void ReadCleanAreaSettings(InitializationFile file)
        {
            CleanAreaSettings = new CleanAreaSettings
            {
                EnableBackupUnit = file.ReadBoolean(CleanAreaGroupName, "EnableBackupUnit")
            };
        }

        private void ReadRedirectTrafficSettings(InitializationFile file)
        {
            RedirectTrafficSettings = new RedirectTrafficSettings
            {
                PlaceSceneryItems = file.ReadBoolean(RedirectTrafficGroupName, "PlaceSceneryItems", true),
                AlwaysPlaceLight = file.ReadBoolean(RedirectTrafficGroupName, "AlwaysPlaceLight", true)
            };
        }

        private void ReadCloseRoadSettings(InitializationFile file)
        {
            CloseRoadSettings = new CloseRoadSettings
            {
                PlaceBarriers = file.ReadBoolean(CloseRoadGroupName, "PlaceBarriers", true),
                AutoCleanBodies = file.ReadBoolean(CloseRoadGroupName, "AutoCleanBodies", true),
                AutoCleanWrecks = file.ReadBoolean(CloseRoadGroupName, "AutoCleanWrecks", true)
            };
        }

        private Keys ValueToKey(string value)
        {
            Keys key;

            if (!Enum.TryParse(value, true, out key))
                _rage.LogTrivial("Failed to parse key in settings file with value: " + value);

            return key;
        }
    }
}