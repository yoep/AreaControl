using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.AbstractionLayer;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Callouts
{
    public class CalloutManager : ICalloutManager
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;

        public CalloutManager(ILogger logger, IRage rage)
        {
            _logger = logger;
            _rage = rage;
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
            _rage.NewSafeFiber(() =>
            {
                GameFiber.Sleep(1000);
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
            }, "RegisterCallouts");
        }
    }
}