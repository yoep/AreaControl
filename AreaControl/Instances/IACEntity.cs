using System.Diagnostics.CodeAnalysis;
using Rage;

namespace AreaControl.Instances
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IACEntity : IDeletable
    {
        /// <summary>
        /// Get the id of this instance.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Check if the instance reference is still valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Check if the instance reference is invalid.
        /// </summary>
        bool IsInvalid { get; }

        /// <summary>
        /// Create a blip for this instance.
        /// </summary>
        void CreateBlip();
        
        /// <summary>
        /// Delete the active blip for this instance.
        /// </summary>
        void DeleteBlip();
    }
}