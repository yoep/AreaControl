using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using AreaControl.Callouts.Riot;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Callouts
{
    public class CalloutManager : ICalloutManager
    {
        private readonly ILogger _logger;

        public CalloutManager(ILogger logger)
        {
            _logger = logger;
        }

        #region Properties

        /// <inheritdoc />
        public IList<Type> Callouts => new List<Type>
        {
            typeof(RiotCallout)
        };

        #endregion

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _logger.Debug("Initializing Callout Manager...");
            _logger.Debug($"Registering {Callouts.Count} callouts...");

            foreach (var callout in Callouts)
            {
                try
                {
                    Functions.RegisterCallout(callout);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to register callout '{callout}' with error: {ex.Message}", ex);
                }
            }
        }
    }
}