using System.Collections.Generic;
using System.Linq;
using AreaControl.Model;
using AreaControl.Rage;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Managers
{
    public class EntityManager : IEntityManager
    {
        private readonly IRage _rage;
        private readonly List<ACVehicle> _managedVehicles = new List<ACVehicle>();
        private readonly List<ACPed> _managedPeds = new List<ACPed>();

        #region Constructors

        public EntityManager(IRage rage)
        {
            _rage = rage;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ACVehicle FindVehicleWithinOrCreate(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            Assert.IsPositive(radius, "radius must be a positive number");
            var controlledVehicle = FindAvailableManagedVehicle(position, radius);

            if (controlledVehicle != null)
                return controlledVehicle;

            var vehicle = VehicleQuery.FindWithin(position, radius);

            return vehicle != null ? RegisterVehicle(vehicle) : CreateVehicleWithOccupants(GetStreetWithinRadius(position, radius));
        }

        #endregion

        #region Functions

        private ACVehicle FindAvailableManagedVehicle(Vector3 position, float radius)
        {
            return _managedVehicles
                .Where(e => IsVehicleWithinRadius(position, radius, e))
                .FirstOrDefault(e => !e.IsBusy);
        }

        private ACVehicle RegisterVehicle(Vehicle vehicle)
        {
            var registeredVehicle = new ACVehicle(vehicle);
            var driver = vehicle.Driver;

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
            var registeredPed = new ACPed(ped);

            _managedPeds.Add(registeredPed);
            Functions.SetPedAsCop(ped);
            Functions.SetCopAsBusy(ped, true);

            return registeredPed;
        }

        private static bool IsVehicleWithinRadius(Vector3 position, float radius, ACVehicle vehicle)
        {
            return vehicle.Instance.DistanceTo(position) <= radius;
        }

        private static Vector3 GetStreetWithinRadius(Vector3 position, float radius)
        {
            return World.GetNextPositionOnStreet(position.Around(radius));
        }

        private ACVehicle CreateVehicleWithOccupants(Vector3 spawnPosition)
        {
            var closestRoad = RoadUtil.GetClosestRoad(spawnPosition, RoadType.All);
            var vehicle = RegisterVehicle(new Vehicle("POLICE", closestRoad.Position, closestRoad.Heading));
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
            return new Ped(new global::Rage.Model("s_m_y_cop_01"), spawnPosition, 3f)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
                KeepTasks = true
            };
        }

        #endregion
    }
}