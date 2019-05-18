using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Instances
{
    public class EntityManager : IEntityManager, IDisposable
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly List<ACVehicle> _managedVehicles = new List<ACVehicle>();
        private readonly List<ACPed> _managedPeds = new List<ACPed>();
        private readonly List<Vehicle> _disposedWrecks = new List<Vehicle>();
        private bool _isActive = true;
        private long _lastInstanceId;

        #region Constructors

        public EntityManager(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ACVehicle FindVehicleWithinOrCreateAt(Vector3 position, Vector3 spawnPosition, float radius, int numberOfOccupantsToSpawn)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius must be a positive number");
            Assert.IsPositive(numberOfOccupantsToSpawn, "numberOfOccupantsToSpawn must be a positive number");
            var controlledVehicle = FindAvailableManagedVehicle(position, radius);

            if (controlledVehicle != null)
            {
                _rage.LogTrivialDebug("Returning controlled vehicle " + controlledVehicle);
                return controlledVehicle;
            }

            var vehicle = FindAvailablePoliceVehicleInWorld(position, radius, numberOfOccupantsToSpawn);

            return vehicle != null ? RegisterVehicle(vehicle) : CreateVehicleWithOccupants(GetStreetAt(spawnPosition), numberOfOccupantsToSpawn);
        }

        /// <inheritdoc />
        public ACVehicle FindManagedVehicle(Vehicle instance)
        {
            return _managedVehicles.FirstOrDefault(x => x.Instance == instance);
        }

        /// <inheritdoc />
        public IReadOnlyList<ACPed> FindPedsWithin(Vector3 position, float radius)
        {
            var peds = new List<ACPed>();

            //find managed peds
            var managedPeds = _managedPeds
                .Where(x => IsPedWithinRadius(position, radius, x))
                .Where(x => x.Instance.IsValid())
                .ToList();
            _rage.LogTrivialDebug($"Found {managedPeds.Count} managed cops which are available");

            peds.AddRange(managedPeds);

            //find game world peds
            var worldPeds = PedQuery.FindCopsWithin(position, radius)
                .Where(x => x.IsValid() && x.IsAlive)
                .Where(x => !IsPedAlreadyManaged(x))
                .ToList();
            _rage.LogTrivialDebug($"Found {worldPeds.Count} cops in the world which are available");

            foreach (var worldPed in worldPeds)
            {
                peds.Add(RegisterPed(worldPed));
            }

            return peds;
        }

        /// <inheritdoc />
        public IReadOnlyList<ACVehicle> GetAllManagedVehicles()
        {
            return _managedVehicles.AsReadOnly();
        }

        /// <inheritdoc />
        public IReadOnlyList<Vehicle> GetAllDisposedWrecks()
        {
            return _disposedWrecks.AsReadOnly();
        }

        /// <inheritdoc />
        public void RegisterDisposedWreck(Vehicle instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            _disposedWrecks.Add(instance);
        }

        /// <inheritdoc />
        public void Dismiss()
        {
            _rage.NewSafeFiber(() =>
            {
                _logger.Trace("Clearing vehicle blips...");
                foreach (var vehicle in GetAllManagedVehicles())
                {
                    vehicle.DeleteBlip();
                    vehicle.Persistent = false;
                }

                while (_managedVehicles.Any(x => !x.IsWandering))
                {
                    foreach (var vehicle in GetAllManagedVehicles().Where(x => !x.IsWandering && x.AllOccupantsPresent))
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
            _managedVehicles.ForEach(x => x.Delete());
            _managedPeds.ForEach(x => x.Delete());
            _isActive = false;
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            //start a cleanup thread
            _rage.NewSafeFiber(() =>
            {
                while (_isActive)
                {
                    var vehiclesToBeRemoved = _managedVehicles.Where(x => !x.IsValid).ToList();
                    var pedsToBeRemoved = _managedPeds.Where(x => !x.IsValid).ToList();
                    var disposedWrecksToBeRemoved = _disposedWrecks.Where(x => !x.IsValid()).ToList();

                    vehiclesToBeRemoved.ForEach(x => _managedVehicles.Remove(x));
                    pedsToBeRemoved.ForEach(x => _managedPeds.Remove(x));
                    disposedWrecksToBeRemoved.ForEach(x => _disposedWrecks.Remove(x));

                    GameFiber.Sleep(2000);
                }
            }, "EntityManager");
        }

        private Vehicle FindAvailablePoliceVehicleInWorld(Vector3 position, float radius, int numberOfOccupantsToSpawn)
        {
            var vehicles = VehicleQuery.FindPoliceVehiclesWithin(position, radius);

            return vehicles.FirstOrDefault(x => !IsVehicleAlreadyManaged(x) && x.Occupants.Length == numberOfOccupantsToSpawn);
        }

        private bool IsVehicleAlreadyManaged(Vehicle instance)
        {
            return _managedVehicles.Any(x => x.Instance == instance);
        }

        private bool IsPedAlreadyManaged(Ped instance)
        {
            return _managedPeds.Any(x => x.Instance == instance);
        }

        private ACVehicle FindAvailableManagedVehicle(Vector3 position, float radius)
        {
            _rage.LogTrivialDebug("Searching for managed vehicle at position " + position);
            return _managedVehicles
                .Where(e => e.IsValid)
                .Where(e => IsVehicleWithinRadius(position, radius, e))
                .FirstOrDefault(e => !e.IsBusy);
        }

        private ACVehicle RegisterVehicle(Vehicle vehicle)
        {
            var registeredVehicle = new ACVehicle(vehicle, ++_lastInstanceId);
            var driver = vehicle.Driver;

            _rage.LogTrivialDebug("Registering a new vehicle in entity manager " + registeredVehicle);
            _managedVehicles.Add(registeredVehicle);

            foreach (var occupant in vehicle.Occupants)
            {
                var ped = RegisterPed(occupant);

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

        private ACPed RegisterPed(Ped ped)
        {
            var registeredPed = new ACPed(ped, ++_lastInstanceId);

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

            var vehicle = GetManagedInstanceForVehicle(lastVehicle) ?? RegisterVehicle(lastVehicle);

            ped.LastVehicle = vehicle;
        }

        private ACVehicle GetManagedInstanceForVehicle(Vehicle vehicle)
        {
            return GetAllManagedVehicles()
                .FirstOrDefault(x => x.Instance == vehicle);
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

        private ACVehicle CreateVehicleWithOccupants(Vector3 spawnPosition, int numberOfOccupantsToSpawn)
        {
            var closestRoad = RoadUtils.GetClosestRoad(spawnPosition, RoadType.All);
            var vehicle = RegisterVehicle(new Vehicle(ModelUtils.GetLocalVehicle(spawnPosition), closestRoad.Position, closestRoad.Lanes.First().Heading));

            for (var i = 0; i < numberOfOccupantsToSpawn; i++)
            {
                var ped = RegisterPed(CreatePed(spawnPosition));

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

        private static Ped CreatePed(Vector3 spawnPosition)
        {
            return new Ped(ModelUtils.GetLocalPed(spawnPosition), spawnPosition, 3f)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                KeepTasks = true
            };
        }

        #endregion
    }
}