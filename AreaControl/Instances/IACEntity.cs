using Rage;

namespace AreaControl.Instances
{
    public interface IACEntity : IDeletable
    {
        /// <summary>
        /// Get the id of this instance.
        /// </summary>
        long Id { get; }
    }
}