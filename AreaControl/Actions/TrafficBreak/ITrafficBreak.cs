using AreaControl.Menu;

namespace AreaControl.Actions.TrafficBreak
{
    public interface ITrafficBreak : IMenuComponent
    {
        /// <summary>
        /// Get of the slow down traffic is active or not.
        /// </summary>
        bool IsActive { get; }
    }
}