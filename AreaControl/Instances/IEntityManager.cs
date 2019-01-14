using System.Collections.Generic;
using Rage;

namespace AreaControl.Instances
{
    public interface IEntityManager
    {
        /// <summary>
        /// Find a <see cref="Vehicle"/> within the given range of the position.
        /// If no <see cref="Vehicle"/> found within the range, create a new one.
        /// </summary>
        /// <param name="position">Set the position to search from.</param>
        /// <param name="spawnPosition">Set the spawn position for the created vehicle.</param>
        /// <param name="radius">Set the range around the position to search within.</param>
        /// <param name="numberOfOccupantsToSpawn">Set the number of occupants to spawn if it needs to be created.</param>
        /// <returns>Returns a Area Controlled vehicle within the given range of the position.</returns>
        ACVehicle FindVehicleWithinOrCreateAt(Vector3 position, Vector3 spawnPosition, float radius, int numberOfOccupantsToSpawn);

        /// <summary>
        /// Get the managed vehicle for the given game vehicle if found.
        /// </summary>
        /// <param name="instance">Set the game vehicle instance.</param>
        /// <returns>Returns the managed vehicle if found, else null.</returns>
        ACVehicle FindManagedVehicle(Vehicle instance);

        /// <summary>
        /// Get all current managed vehicles from this entity manager.
        /// </summary>
        /// <returns>Returns the list of managed vehicles.</returns>
        IReadOnlyList<ACVehicle> GetAllManagedVehicles();

        /// <summary>
        /// Get all current disposed wrecks.
        /// </summary>
        /// <returns>Returns the list of disposed wrecks.</returns>
        IReadOnlyList<Vehicle> GetAllDisposedWrecks();

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