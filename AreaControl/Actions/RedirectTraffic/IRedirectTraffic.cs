using AreaControl.Menu;

namespace AreaControl.Actions.RedirectTraffic
{
    public interface IRedirectTraffic : IMenuComponent
    {
        /// <summary>
        /// Get if the redirect traffic is active.
        /// </summary>
        bool IsActive { get; }
    }
}