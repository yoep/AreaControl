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

        private readonly IRage _rage;

        public SettingsManager(IRage rage)
        {
            _rage = rage;
        }

        /// <inheritdoc />
        public GeneralSettings GeneralSettings { get; private set; }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void LoadSettings()
        {
            try
            {
                var settingsFile = new InitializationFile(File);

                ReadGeneralSettings(settingsFile);
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("Failed to load settings with " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void ReadGeneralSettings(InitializationFile settingsFile)
        {
            GeneralSettings = new GeneralSettings
            {
                OpenMenuKey = ValueToKey(settingsFile.ReadString("General", "OpenMenuKey", "C")),
                OpenMenuModifierKey = ValueToKey(settingsFile.ReadString("General", "OpenMenuModifierKey", "None"))
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