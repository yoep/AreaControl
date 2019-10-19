using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Menu.Response;
using Rage;

namespace AreaControl.Duties
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IDutyManager
    {
        /// <summary>
        /// Listen for a new available duty around the given position.
        /// Will return a new listener that will be triggered when a new duty becomes available around the given position.
        /// </summary>
        /// <param name="ped">Set the ped to listen for new duties.</param>
        IDutyListener this[ACPed ped] { get; }

        #region Properties
        
        /// <summary>
        /// Get the current duty ID of this manager.
        /// </summary>
        long DutyId { get; }
        
        /// <summary>
        /// Get a list of all registered duties.
        /// </summary>
        IReadOnlyList<IDuty> RegisteredDuties { get; }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Get the next available duty in the surrounding area of the ped.
        /// The list of duties is fetched based on the <see cref="PedType"/> of the ped.
        /// </summary>
        /// <param name="ped">The ped to get the next available duty for.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty NextAvailableDuty(ACPed ped);

        /// <summary>
        /// Get the next available duty in the surrounding area of the ped based on the given duties.
        /// </summary>
        /// <param name="ped">The ped to get the next available duty for.</param>
        /// <param name="types">The duty types to check that are available in the area of the ped.</param>
        /// <returns>Returns the next available duty if available, else null.</returns>
        IDuty NextAvailableDuty(ACPed ped, DutyTypeFlag types);

        /// <summary>
        /// Create a new redirect traffic duty.
        /// </summary>
        /// <param name="ped">The ped that needs to execute the duty.</param>
        /// <param name="position">The position of the redirect traffic duty.</param>
        /// <param name="heading">The heading that the ped must face.</param>
        /// <param name="responseCode">The response code for the duty.</param>
        /// <returns>Returns the registered duty instance.</returns>
        IDuty NewRedirectTrafficDuty(ACPed ped, Vector3 position, float heading, ResponseCode responseCode);
        
        /// <summary>
        /// Create a new place objects duty.
        /// </summary>
        /// <param name="ped">The ped that needs to execute the duty.</param>
        /// <param name="objects">The objects that need to be placed.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="placeFromHand">Set if the objects need to be placed from the ped hand or not.</param>
        /// <returns>Returns the registered duty instance.</returns>
        IDuty NewPlaceObjectsDuty(ACPed ped, IEnumerable<PlaceObjectsDuty.PlaceObject> objects, ResponseCode responseCode, bool placeFromHand);

        /// <summary>
        /// Abort all active duties and clean them from the duty manager so they can be instantiated again.
        /// </summary>
        void DismissDuties();
        
        #endregion
    }
}