using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace AreaControl.Callouts
{
    [CalloutInfo("Riot", CalloutProbability.Low)]
    public class RiotCallout : Callout
    {
        private static readonly Vector3 AllowedArea = new Vector3(-152.45f, -1133.04f, 24.09f);
        private const float Distance = 200f;

        private readonly ILogger _logger = IoC.Instance.GetInstance<ILogger>();
        private readonly IComputerPlus _computerPlus = IoC.Instance.GetInstance<IComputerPlus>();
        private readonly List<ACPed> _mobs = new List<ACPed>();
        private readonly List<Vehicle> _spawnedVehicles = new List<Vehicle>();
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
                _logger.Trace("Player is not within Riot callout zone");
                return false;
            }

            _logger.Trace("Player is within Riot callout zone");
            var initialSpawnPoint = AllowedArea.Around2D(0f, Distance);
            _spawnPoint = RoadUtils.GetClosestRoad(initialSpawnPoint, RoadType.MajorRoadsOnly).Position;

            AddMinimumDistanceCheck(10f, _spawnPoint);
            ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 30f);
            CalloutMessage = "Riot in progress";
            CalloutPosition = _spawnPoint;
            CreateCalloutData();
            Functions.PlayScannerAudioUsingPosition("WE_HAVE A RIOT IN_OR_ON_POSITION ALL_UNITS_RESPOND_CODE_99_EMERGENCY", _spawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        /// <inheritdoc />
        public override bool OnCalloutAccepted()
        {
            var numberOfPeds = _random.Next(3, 15);

            _computerPlus.UpdateCalloutStatus(_calloutGuid, CPCallStatus.Unit_Responding);

            for (var i = 0; i < numberOfPeds; i++)
            {
                var acPed = CreatePed(i);

                acPed.CreateBlip();
                _mobs.Add(acPed);
            }

            CreateScenery();

            _logger.Trace($"Created a total of {_mobs.Count} riot members");
            return base.OnCalloutAccepted();
        }

        /// <inheritdoc />
        public override void OnCalloutNotAccepted()
        {
            _computerPlus.UpdateCalloutStatus(_calloutGuid, CPCallStatus.Cancelled);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            var idleMobs = _mobs.Where(x => !x.HasTarget).ToList();

            if (idleMobs.Count == 0)
                return;

            var entities = EntityQuery.FindAliveEntitiesWithin(_spawnPoint, 20f)
                .Where(x => !IsMobEntity(x))
                .OfType<Ped>()
                .ToList();

            if (entities.Count > 0)
            {
                _logger.Trace($"Riot, found {entities.Count} nearby entities for {idleMobs.Count} idling riot mobs");
                foreach (var mob in idleMobs)
                {
                    mob.AttackTarget(entities[0]);
                }
            }
            else
            {
                _logger.Trace("Riot, moving riot group to new location...");
                MoveGroupToNewLocation();
            }
        }

        /// <inheritdoc />
        public override void End()
        {
            base.End();

            _computerPlus.UpdateCalloutStatus(_calloutGuid, CPCallStatus.Completed);
            _mobs.ForEach(ped => ped.Delete());
            _spawnedVehicles.ForEach(x => x.Dismiss());
        }

        #endregion

        #region Functions

        private ACPed CreatePed(int number)
        {
            var model = ModelUtils.GetRiotPedModel();

            model.LoadAndWait();

            var ped = new Ped(model, _spawnPoint.Around2D(0f, 5f), 0f)
            {
                RelationshipGroup = RelationshipGroup.AmbientGangHillbilly,
                CanAttackFriendlies = false,
                IsPersistent = true,
                StaysInGroups = true,
                KeepTasks = true
            };

            Functions.SetPedResistanceChance(ped, 10f);
            ped.Inventory.GiveNewWeapon(new WeaponAsset(GetWeaponForPed()), -1, true);

            return new ACPed(ped, number)
            {
                IsFriendly = false
            };
        }

        private bool IsMobEntity(Entity entity)
        {
            return _mobs.Any(x => x.Instance == entity);
        }

        private void CreateCalloutData()
        {
            _calloutGuid = _computerPlus.CreateCallout("Riot in progress", "Riot", _spawnPoint, CPResponseType.Code_3);
        }

        private void CreateScenery()
        {
            var numberOfVehiclesOnFire = _random.Next(1, 5);
            var numberOfVehiclesBurnedOut = _random.Next(1, 10);

            _logger.Trace($"Creating {numberOfVehiclesOnFire} vehicle on fire");
            for (var i = 0; i < numberOfVehiclesOnFire; i++)
            {
                var model = ModelUtils.GetRiotVehicleModel();

                model.LoadAndWait();

                var vehicle = new Vehicle(model, _spawnPoint.Around2D(30f))
                {
                    EngineHealth = _random.Next(0, 300),
                    IsFireProof = false,
                    IsOnFire = true,
                    IsPersistent = true
                };

                DamageVehicle(vehicle);
                _spawnedVehicles.Add(vehicle);
            }

            _logger.Trace($"Creating {numberOfVehiclesBurnedOut} vehicle that have been burned out");
            for (var i = 0; i < numberOfVehiclesBurnedOut; i++)
            {
                var model = ModelUtils.GetRiotVehicleModel();

                model.LoadAndWait();

                var vehicle = new Vehicle(model, _spawnPoint.Around2D(30f))
                {
                    EngineHealth = 0f,
                    FuelTankHealth = _random.Next(50, 600),
                    Health = 10,
                    IsPersistent = true
                };

                DamageVehicle(vehicle);
                _spawnedVehicles.Add(vehicle);
            }
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
                    return "WEAPON_BAT";
            }
        }

        private void MoveGroupToNewLocation()
        {
            var mobs = _mobs
                .Where(x => x.IsValid && !Functions.IsPedArrested(x.Instance))
                .ToList();

            if (mobs.Count == 0)
                return;

            var closestRoad = RoadUtils.GetClosestRoad(mobs.First().Instance.Position, RoadType.MajorRoadsOnly);
            var moveDirection = MathHelper.ConvertHeadingToDirection(closestRoad.Lanes[0].Heading);
            var newRoad = RoadUtils.GetClosestRoad(closestRoad.Position + moveDirection * 30f, RoadType.MajorRoadsOnly);
            
            foreach (var mob in mobs)
            {
                mob.RunTo(newRoad.Position, newRoad.Lanes[0].Heading);
            }
        }

        private void DamageVehicle(Vehicle vehicle)
        {
            vehicle.Deform(Vector3.UnitX, _random.Next(1, 20), _random.Next(5, 20));
        }

        private static bool IsPlayerWithinAllowedArea()
        {
            var playerPosition = Game.LocalPlayer.Character.Position;

            return Vector3.Distance(AllowedArea, playerPosition) <= Distance;
        }

        #endregion
    }
}