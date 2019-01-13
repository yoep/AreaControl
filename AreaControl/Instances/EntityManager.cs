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
        private readonly List<ACVehicle> _managedVehicles = new List<ACVehicle>();
        private readonly List<ACPed> _managedPeds = new List<ACPed>();
        private readonly List<Vehicle> _disposedWrecks = new List<Vehicle>();
        private bool _isActive = true;
        private long _lastInstanceId;

        #region Constructors

        public EntityManager(IRage rage)
        {
            _rage = rage;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ACVehicle FindVehicleWithinOrCreateAt(Vector3 position, Vector3 spawnPosition, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius must be a positive number");
            var controlledVehicle = FindAvailableManagedVehicle(position, radius);

            if (controlledVehicle != null)
            {
                _rage.LogTrivialDebug("Returning controlled vehicle " + controlledVehicle);
                return controlledVehicle;
            }

            var vehicle = FindAvailablePoliceVehicleInWorld(position, radius);

            return vehicle != null ? RegisterVehicle(vehicle) : CreateVehicleWithOccupants(GetStreetAt(spawnPosition));
        }

        /// <inheritdoc />
        public ACVehicle FindManagedVehicle(Vehicle instance)
        {
            return _managedVehicles.FirstOrDefault(x => x.Instance == instance);
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
                while (_managedVehicles.Any(x => !x.IsWandering))
                {
                    foreach (var vehicle in _managedVehicles.Where(x => !x.IsWandering && x.AllOccupantsPresent))
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
                    var vehiclesToBeRemoved = _managedVehicles.Where(x => !x.Instance.IsValid()).ToList();
                    var pedsToBeRemoved = _managedPeds.Where(x => !x.Instance.IsValid()).ToList();
                    var disposedWrecksToBeRemoved = _disposedWrecks.Where(x => !x.IsValid()).ToList();

                    vehiclesToBeRemoved.ForEach(x => _managedVehicles.Remove(x));
                    pedsToBeRemoved.ForEach(x => _managedPeds.Remove(x));
                    disposedWrecksToBeRemoved.ForEach(x => _disposedWrecks.Remove(x));

                    GameFiber.Sleep(30000);
                }
            }, "EntityManager");
        }

        private Vehicle FindAvailablePoliceVehicleInWorld(Vector3 position, float radius)
        {
            var vehicles = VehicleQuery.FindPoliceVehiclesWithin(position, radius);

            return vehicles.FirstOrDefault(x => !IsVehicleAlreadyManaged(x));
        }

        private bool IsVehicleAlreadyManaged(Vehicle instance)
        {
            return _managedVehicles.Any(x => x.Instance == instance);
        }

        private ACVehicle FindAvailableManagedVehicle(Vector3 position, float radius)
        {
            _rage.LogTrivialDebug("Searching for managed vehicle at position " + position);
            return _managedVehicles
                .Where(e => e.Instance.IsValid())
                .Where(e => IsVehicleWithinRadius(position, radius, e))
                .FirstOrDefault(e => !e.IsBusy);
        }

        private ACVehicle RegisterVehicle(Vehicle vehicle)
        {
            var registeredVehicle = new ACVehicle(vehicle, GetNextId());
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
            var registeredPed = new ACPed(ped, GetNextId());

            _managedPeds.Add(registeredPed);
            Functions.SetPedAsCop(ped);
            Functions.SetCopAsBusy(ped, true);

            return registeredPed;
        }

        private long GetNextId()
        {
            _lastInstanceId++;
            return _lastInstanceId;
        }

        private static bool IsVehicleWithinRadius(Vector3 position, float radius, ACVehicle vehicle)
        {
            return vehicle.Instance.DistanceTo(position) <= radius;
        }

        private static Vector3 GetStreetWithinRadius(Vector3 position, float radius)
        {
            return World.GetNextPositionOnStreet(position.Around(radius));
        }

        private static Vector3 GetStreetAt(Vector3 position)
        {
            return World.GetNextPositionOnStreet(position);
        }

        private ACVehicle CreateVehicleWithOccupants(Vector3 spawnPosition)
        {
            var closestRoad = RoadUtil.GetClosestRoad(spawnPosition, RoadType.All);
            var vehicle = RegisterVehicle(new Vehicle("POLICE", closestRoad.Position, closestRoad.Lanes.First().Heading));
            var driver = RegisterPed(CreatePed(spawnPosition));
            driver.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
            var passenger = RegisterPed(CreatePed(spawnPosition));
            passenger.WarpIntoVehicle(vehicle, VehicleSeat.RightFront);

            vehicle.Driver = driver;
            vehicle.Passengers.Add(passenger);
            vehicle.CreateBlip();

            return vehicle;
        }

        private static Ped CreatePed(Vector3 spawnPosition)
        {
            return new Ped(new Model("s_m_y_cop_01"), spawnPosition, 3f)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                KeepTasks = true
            };
        }

        #endregion
    }
}