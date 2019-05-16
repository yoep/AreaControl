using System;
using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Functions = LSPD_First_Response.Mod.API.Functions;

namespace AreaControl.Callouts
{
    [CalloutInfo("RiotCallout", CalloutProbability.Low)]
    public class RiotCallout : Callout
    {
        private static readonly Vector3 AllowedArea = new Vector3(-152.45f, -1133.04f, 24.09f);
        private const float Distance = 1000f;

        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();
        private readonly List<ACPed> _mobs = new List<ACPed>();
        private readonly Random _random = new Random();
        private Vector3 _spawnPoint;
        private Guid _calloutGuid;

        #region Methods

        /// <inheritdoc />
        public override bool OnBeforeCalloutDisplayed()
        {
            _logger.Trace("Checking if Riot callout is allowed to be triggered...");

            if (!IsPlayerWithinAllowedArea())
            {
                _logger.Trace($"Player is not within Riot callout zone");
                return false;
            }

            _logger.Trace("Player is within Riot callout zone");
            var initialSpawnPoint = AllowedArea.Around2D(0f, Distance);
            _spawnPoint = RoadUtils.GetClosestRoad(initialSpawnPoint, RoadType.All).Position;

            AddMinimumDistanceCheck(5f, _spawnPoint);
            ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 15f);
            CalloutMessage = "Riot in progress";
            CalloutPosition = _spawnPoint;
            CreateCalloutData();
            Functions.PlayScannerAudioUsingPosition("WE_HAVE A RIOT IN_OR_ON_POSITION ALL_UNITS_RESPOND_CODE_99_EMERGENCY", _spawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            var numberOfPeds = _random.Next(1, 10);

//            ComputerPlus.API.Functions.UpdateCalloutStatus(_calloutGuid, ECallStatus.Unit_Responding);

            for (var i = 0; i < numberOfPeds; i++)
            {
                var acPed = CreatePed(i);

                acPed.CreateBlip();
                _mobs.Add(acPed);
            }

            _logger.Trace($"Created a total of {_mobs.Count} riot members");
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
//            ComputerPlus.API.Functions.UpdateCalloutStatus(_calloutGuid, ECallStatus.Cancelled);

            base.OnCalloutNotAccepted();
        }

        public override void End()
        {
            base.End();

//            ComputerPlus.API.Functions.UpdateCalloutStatus(_calloutGuid, ECallStatus.Completed);
            _mobs.ForEach(ped => ped.Delete());
        }

        #endregion

        #region Functions

        private ACPed CreatePed(int number)
        {
            var ped = new Ped(_spawnPoint.Around2D(0f, 5f))
            {
                RelationshipGroup = RelationshipGroup.AmbientGangHillbilly,
                CanAttackFriendlies = false,
                IsPersistent = true,
                StaysInGroups = true,
                KeepTasks = true
            };

            Functions.SetPedResistanceChance(ped, 10f);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(GetWeaponForPed()), -1, false);

            return new ACPed(ped, number)
            {
                IsFriendly = false
            };
        }

        private void CreateCalloutData()
        {
//            _calloutGuid = ComputerPlus.API.Functions.CreateCallout(new CalloutData("Riot in progress", "Riot", _spawnPoint, EResponseType.Code_3));
        }

        private string GetWeaponForPed()
        {
            switch (_random.Next(1, 3))
            {
                case 1:
                    return "WEAPON_BAT";
                case 2:
                    return "WEAPON_MOLOTOV";
                default:
                    return "WEAPON_UNARMED";
            }
        }

        private static bool IsPlayerWithinAllowedArea()
        {
            var playerPosition = Game.LocalPlayer.Character.Position;

            return Vector3.Distance(AllowedArea, playerPosition) <= Distance;
        }

        #endregion
    }
}