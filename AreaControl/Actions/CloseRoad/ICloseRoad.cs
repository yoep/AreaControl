using AreaControl.Menu;

namespace AreaControl.Actions.CloseRoad
{
    public interface ICloseRoad : IMenuComponent
    {
        /// <summary>
        /// Get if the action is currently being executed.
        /// </summary>
        bool IsActive { get; }
    }
}