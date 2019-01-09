using AreaControl.Menu;

namespace AreaControl.Actions.CloseRoad
{
    public interface ICloseRoad : IMenuComponent
    {
        /// <summary>
        /// Get if the action is currently being executed.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// End the close road action and reopens the closed road.
        /// </summary>
        void OpenRoad();
    }
}