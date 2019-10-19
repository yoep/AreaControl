using System.Collections.Generic;
using Rage;

namespace AreaControl.Instances
{
    public interface IEntityManager
    {
        /// <summary>
        /// Get all managed vehicles by this entity manager.
        /// </summary>
        List<ACVehicle> ManagedVehicles { get; }

        /// <summary>
        /// Get disposed wrecks.
        /// </summary>
        List<Vehicle> DisposedWrecks { get; }

        /// <summary>
        /// Find a <see cref="Vehicle"/> within the given range of the position.
        /// If no <see cref="Vehicle"/> found within the range, create a new one.
        /// </summary>
        /// <param name="position">Set the position to search from.</param>
        /// <param name="spawnPosition">Set the spawn position for the created vehicle.</param>
        /// <param name="type">The vehicle type to search for.</param>
        /// <param name="radius">Set the range around the position to search within.</param>
        /// <param name="numberOfOccupantsToSpawn">Set the number of occupants to spawn if it needs to be created.</param>
        /// <returns>Returns a Area Controlled vehicle within the given range of the position.</returns>
        ACVehicle FindVehicleWithinOrCreateAt(Vector3 position, Vector3 spawnPosition, VehicleType type, float radius, int numberOfOccupantsToSpawn);

        /// <summary>
        /// Get the managed vehicle for the given game vehicle if found.
        /// </summary>
        /// <param name="instance">Set the game vehicle instance.</param>
        /// <returns>Returns the managed vehicle if found, else null.</returns>
        ACVehicle FindManagedVehicle(Vehicle instance);

        /// <summary>
        /// Find <see cref="Ped"/>'s within the given range of the position.
        /// </summary>
        /// <param name="position">The position to search around.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <param name="type">The ped type to find.</param>
        /// <returns>Returns a list of managed peds within the given area if found, else an empty list.</returns>
        IReadOnlyList<ACPed> FindPedsWithin(Vector3 position, float radius, PedType type);
        
        /// <summary>
        /// Create a <see cref="Vehicle"/> at the given spawn position.
        /// </summary>
        /// <param name="spawnPosition">The spawn position for the created vehicle.</param>
        /// <param name="type">The type of the vehicle to spawn.</param>
        /// <param name="numberOfOccupantsToSpawn">The number of occupants the vehicle should spawn with.</param>
        /// <returns>Returns the created vehicle with occupants.</returns>
        ACVehicle CreateVehicleAt(Vector3 spawnPosition, VehicleType type, int numberOfOccupantsToSpawn);

        /// <summary>
        /// Register the given vehicle instance as a disposed wreck.
        /// </summary>
        /// <param name="instance">Set the instance to register.</param>
        void RegisterDisposedWreck(Vehicle instance);

        /// <summary>
        /// Dismiss all managed vehicles and let them wander around again.
        /// This will make the vehicles and peds be managed by Rage, but this manager will still keep the reference to the instances for potential later use.
        /// </summary>
        void Dismiss();
    }
}