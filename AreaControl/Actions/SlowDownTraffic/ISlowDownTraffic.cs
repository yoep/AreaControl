using AreaControl.Menu;

namespace AreaControl.Actions.SlowDownTraffic
{
    public interface ISlowDownTraffic : IMenuComponent
    {
        /// <summary>
        /// Get of the slow down traffic is active or not.
        /// </summary>
        bool IsActive { get; }
    }
}