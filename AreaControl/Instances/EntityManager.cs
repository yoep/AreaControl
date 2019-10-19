using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using AreaControl.Utils.Road;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Instances
{
    public class EntityManager : IEntityManager, IDisposable
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly List<ACPed> _managedPeds = new List<ACPed>();
        private bool _isActive = true;
        private long _lastInstanceId;

        #region Constructors

        public EntityManager(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public List<ACVehicle> ManagedVehicles { get; } = new List<ACVehicle>();

        /// <inheritdoc />
        public List<Vehicle> DisposedWrecks { get; } = new List<Vehicle>();

        #endregion

        #region Methods

        /// <inheritdoc />
        public ACVehicle FindVehicleWithinOrCreateAt(Vector3 position, Vector3 spawnPosition, VehicleType type, float radius, int numberOfOccupantsToSpawn)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.NotNull(type, "type cannot be null");
            Assert.IsPositive(radius, "radius must be a positive number");
            Assert.IsPositive(numberOfOccupantsToSpawn, "numberOfOccupantsToSpawn must be a positive number");
            var controlledVehicle = FindAvailableManagedVehicle(position, type, radius);

            if (controlledVehicle != null)
            {
                _rage.LogTrivialDebug("Returning controlled vehicle " + controlledVehicle);
                return controlledVehicle;
            }

            var vehicle = FindAvailablePoliceVehicleInWorld(position, radius, numberOfOccupantsToSpawn);

            return vehicle != null
                ? RegisterVehicle(vehicle, type)
                : CreateVehicleWithOccupants(GetStreetAt(spawnPosition), type, numberOfOccupantsToSpawn);
        }

        /// <inheritdoc />
        public ACVehicle FindManagedVehicle(Vehicle instance)
        {
            return ManagedVehicles.FirstOrDefault(x => x.Instance == instance);
        }

        /// <inheritdoc />
        public IReadOnlyList<ACPed> FindPedsWithin(Vector3 position, float radius, PedType type)
        {
            var peds = new List<ACPed>();

            // find managed peds
            var managedPeds = _managedPeds
                .Where(x => IsPedWithinRadius(position, radius, x) && x.Type == type)
                .Where(x => x.Instance.IsValid())
                .ToList();
            _logger.Trace($"Found {managedPeds.Count} managed {type} which are available in the area");

            peds.AddRange(managedPeds);

            // only search in the game world when the type is Cop
            if (type == PedType.Cop)
                peds.AddRange(FindCopsInGameWorld(position, radius));

            return peds;
        }

        /// <inheritdoc />
        public ACVehicle CreateVehicleAt(Vector3 spawnPosition, VehicleType type, int numberOfOccupantsToSpawn)
        {
            return CreateVehicleWithOccupants(spawnPosition, type, numberOfOccupantsToSpawn);
        }

        /// <inheritdoc />
        public ACVehicle CreateVehicleAt(Vector3 spawnPosition, float heading, VehicleType type, int numberOfOccupantsToSpawn)
        {
            return CreateVehicleWithOccupants(spawnPosition, heading, type, numberOfOccupantsToSpawn);
        }

        /// <inheritdoc />
        public void RegisterDisposedWreck(Vehicle instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            DisposedWrecks.Add(instance);
        }

        /// <inheritdoc />
        public void Dismiss()
        {
            _rage.NewSafeFiber(() =>
            {
                _logger.Trace("Clearing vehicle blips...");
                foreach (var vehicle in ManagedVehicles)
                {
                    vehicle.DeleteBlip();
                    vehicle.Persistent = false;
                }

                while (ManagedVehicles.Any(x => !x.IsWandering))
                {
                    foreach (var vehicle in ManagedVehicles.Where(x => !x.IsWandering && x.AllOccupantsPresent))
                    {
                        vehicle.DisableSirens();
                        vehicle.Wander();
                    }

                    GameFiber.Sleep(500);
                }
            }, "EntityManager.Dismiss");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ManagedVehicles.ForEach(x => x.Delete());
            _managedPeds.ForEach(x => x.Delete());
            _isActive = false;
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            // start a cleanup thread
            _rage.NewSafeFiber(() =>
            {
                while (_isActive)
                {
                    ManagedVehicles.RemoveAll(x => x.IsInvalid);
                    _managedPeds.RemoveAll(x => x.IsInvalid);
                    DisposedWrecks.RemoveAll(x => !x.IsValid());

                    GameFiber.Sleep(2000);
                }
            }, "EntityManager.CleanupThread");
        }

        private Vehicle FindAvailablePoliceVehicleInWorld(Vector3 position, float radius, int numberOfOccupantsToSpawn)
        {
            var vehicles = VehicleQueryUtils.FindPoliceVehiclesWithin(position, radius);

            return vehicles.FirstOrDefault(x => !IsVehicleAlreadyManaged(x) && x.Occupants.Length == numberOfOccupantsToSpawn);
        }

        private bool IsVehicleAlreadyManaged(Vehicle instance)
        {
            return ManagedVehicles.Any(x => x.Instance == instance);
        }

        private bool IsPedAlreadyManaged(Ped instance)
        {
            return _managedPeds.Any(x => x.Instance == instance);
        }

        private ACVehicle FindAvailableManagedVehicle(Vector3 position, VehicleType type, float radius)
        {
            _logger.Trace($"Searching for managed vehicle {type} at position {position}");
            return ManagedVehicles
                .Where(e => e.IsValid && e.Type == type)
                .Where(e => IsVehicleWithinRadius(position, radius, e))
                .FirstOrDefault(e => !e.IsBusy);
        }

        private ACVehicle RegisterVehicle(Vehicle vehicle, VehicleType type)
        {
            var registeredVehicle = new ACVehicle(vehicle, type, ++_lastInstanceId);
            var driver = vehicle.Driver;

            _rage.LogTrivialDebug("Registering a new vehicle in entity manager " + registeredVehicle);
            ManagedVehicles.Add(registeredVehicle);

            foreach (var occupant in vehicle.Occupants)
            {
                var ped = RegisterPed(occupant, VehicleTypeToPedType(type));

                if (occupant == driver)
                {
                    registeredVehicle.Driver = ped;
                }
                else
                {
                    registeredVehicle.Passengers.Add(ped);
                }
            }

            return registeredVehicle;
        }

        private ACPed RegisterPed(Ped ped, PedType type)
        {
            var registeredPed = new ACPed(ped, type, ++_lastInstanceId);

            RegisterLastVehicleForPed(registeredPed);

            _managedPeds.Add(registeredPed);
            Functions.SetPedAsCop(ped);
            Functions.SetCopAsBusy(ped, true);

            return registeredPed;
        }

        private void RegisterLastVehicleForPed(ACPed ped)
        {
            var lastVehicle = ped.Instance.LastVehicle;

            if (lastVehicle == null)
                return;

            var vehicle = GetManagedInstanceForVehicle(lastVehicle) ?? RegisterVehicle(lastVehicle, PedTypeToVehicleType(ped.Type));

            ped.LastVehicle = vehicle;
        }

        private ACVehicle GetManagedInstanceForVehicle(Entity vehicle)
        {
            return ManagedVehicles.FirstOrDefault(x => x.Instance == vehicle);
        }

        private static PedType VehicleTypeToPedType(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.Ambulance:
                    return PedType.Medic;
                case VehicleType.FireTruck:
                    return PedType.Fireman;
                default:
                    return PedType.Cop;
            }
        }

        private static VehicleType PedTypeToVehicleType(PedType type)
        {
            switch (type)
            {
                case PedType.Medic:
                    return VehicleType.Ambulance;
                case PedType.Fireman:
                    return VehicleType.FireTruck;
                default:
                    return VehicleType.Police;
            }
        }

        private IEnumerable<ACPed> FindCopsInGameWorld(Vector3 position, float radius)
        {
            var peds = new List<ACPed>();
            var worldPeds = PedQueryUtils.FindCopsWithin(position, radius)
                .Where(x => x.IsValid() && x.IsAlive)
                .Where(x => !IsPedAlreadyManaged(x))
                .ToList();
            _logger.Trace($"Found {worldPeds.Count} cops in the world which are available in the area");

            foreach (var worldPed in worldPeds)
            {
                peds.Add(RegisterPed(worldPed, PedType.Cop));
            }

            return peds;
        }

        private static bool IsVehicleWithinRadius(Vector3 position, float radius, ACVehicle vehicle)
        {
            return Vector3.Distance2D(position, vehicle.Instance.Position) <= radius;
        }

        private static bool IsPedWithinRadius(Vector3 position, float radius, ACPed ped)
        {
            return Vector3.Distance2D(position, ped.Instance.Position) <= radius;
        }

        private static Vector3 GetStreetWithinRadius(Vector3 position, float radius)
        {
            return World.GetNextPositionOnStreet(position.Around(radius));
        }

        private static Vector3 GetStreetAt(Vector3 position)
        {
            return World.GetNextPositionOnStreet(position);
        }

        private ACVehicle CreateVehicleWithOccupants(Vector3 spawnPosition, VehicleType type, int numberOfOccupantsToSpawn)
        {
            var closestRoad = RoadUtils.GetClosestRoad(spawnPosition, RoadType.All);

            return CreateVehicleWithOccupants(spawnPosition, closestRoad.Lanes.First().Heading, type, numberOfOccupantsToSpawn);
        }

        private ACVehicle CreateVehicleWithOccupants(Vector3 spawnPosition, float heading, VehicleType type, int numberOfOccupantsToSpawn)
        {
            var pedType = VehicleTypeToPedType(type);
            var gameVehicle = new Vehicle(GetVehicleModel(spawnPosition, type), spawnPosition, heading);
            var vehicle = RegisterVehicle(gameVehicle, type);

            for (var i = 0; i < numberOfOccupantsToSpawn; i++)
            {
                var ped = RegisterPed(CreatePed(spawnPosition, pedType), pedType);

                switch (i)
                {
                    case 0:
                        ped.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                        vehicle.Driver = ped;
                        break;
                    case 1:
                        ped.WarpIntoVehicle(vehicle, VehicleSeat.RightFront);
                        vehicle.Passengers.Add(ped);
                        break;
                    default:
                        ped.WarpIntoVehicle(vehicle, VehicleSeat.Any);
                        vehicle.Passengers.Add(ped);
                        break;
                }
            }

            vehicle.CreateBlip();

            return vehicle;
        }

        private static Ped CreatePed(Vector3 spawnPosition, PedType type)
        {
            var model = GetPedModel(spawnPosition, type);

            return new Ped(model, spawnPosition, 3f)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                KeepTasks = true
            };
        }

        private static Model GetVehicleModel(Vector3 spawnPosition, VehicleType type)
        {
            switch (type)
            {
                case VehicleType.FireTruck:
                    return ModelUtils.GetFireTruck();
                case VehicleType.Ambulance:
                    return ModelUtils.GetAmbulance();
                default:
                    return ModelUtils.GetLocalPolice(spawnPosition);
            }
        }

        private static Model GetPedModel(Vector3 spawnPosition, PedType type)
        {
            switch (type)
            {
                case PedType.Fireman:
                    return ModelUtils.GetFireman();
                case PedType.Medic:
                    return ModelUtils.GetMedic();
                case PedType.Cop:
                    return ModelUtils.GetLocalCop(spawnPosition);
                default:
                    return ModelUtils.GetRiotPedModel();
            }
        }

        #endregion
    }
}