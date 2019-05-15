using System;
using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace AreaControl.Callouts.Riot
{
    [CalloutInfo("Riot", CalloutProbability.Low)]
    public class RiotCallout : Callout, IRiotCallout
    {
        private static readonly Vector3 AllowedArea = new Vector3(-152.45f, -1133.04f, 24.09f);
        private const float Distance = 200f;

        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();
        private readonly List<ACPed> _mobs = new List<ACPed>();
        private Vector3 spawnPoint;

        #region Methods

        /// <inheritdoc />
        public override bool OnBeforeCalloutDisplayed()
        {
            _logger.Trace("Checking if Riot callout is allowed to be triggered...");

            if (!IsPlayerWithinAllowedArea())
            {
                _logger.Trace("Player is not within Riot callout zone");
                return false;
            }
           
            _logger.Trace("Player is within Riot callout zone");
            var initialSpawnPoint = AllowedArea.Around2D(0f, Distance);
            spawnPoint = RoadUtils.GetClosestRoad(initialSpawnPoint, RoadType.All).Position;

            AddMinimumDistanceCheck(5f, spawnPoint);
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 15f);
            CalloutMessage = "Riot in progress";
            CalloutPosition = spawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE A RIOT IN_OR_ON_POSITION ALL_UNITS_RESPOND_CODE_99_EMERGENCY", spawnPoint);
            
            return true;
        }

        public override bool OnCalloutAccepted()
        {
            var random = new Random();
            var numberOfPeds = random.Next(1, 10);

            for (var i = 0; i < numberOfPeds; i++)
            {
                var acPed = CreatePed(i);
                
                _mobs.Add(acPed);
            }

            return base.OnCalloutAccepted();
        }

        public override void End()
        {
            _mobs.ForEach(ped => ped.Delete());
            
            base.End();
        }

        #endregion

        #region Functions

        private ACPed CreatePed(int number)
        {
            var ped = new Ped(spawnPoint.Around2D(0f, 5f))
            {
                RelationshipGroup = RelationshipGroup.Gang9,
                CanAttackFriendlies = false,
                IsPersistent = true,
                StaysInGroups = true
            };
            
            return new ACPed(ped, number);
        }

        private static bool IsPlayerWithinAllowedArea()
        {
            var playerPosition = Game.LocalPlayer.Character.Position;

            return Vector3.Distance(AllowedArea, playerPosition) <= Distance;
        }

        #endregion
    }
}