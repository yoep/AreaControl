using AreaControl.Instances;
using LSPD_First_Response.Mod.API;

namespace AreaControl.Duties
{
    /// <summary>
    /// Abstract implementation of a <see cref="IDuty"/> that is aware of pursuits being started and ended in the game world.
    /// This means that the duty can react to a pursuit being started in the game world.
    /// </summary>
    public abstract class AbstractOnPursuitAwareDuty : AbstractDuty
    {
        protected AbstractOnPursuitAwareDuty(long id, ACPed ped)
            : base(id, ped)
        {
            Events.OnPursuitStarted += OnPursuitStarted;
            Events.OnPursuitEnded += OnPursuitEnded;
        }

        /// <summary>
        /// Event that is being invoked when a pursuit is started in the game world.
        /// </summary>
        /// <param name="pursuitHandle">The LSPDFR pursuit handle.</param>
        protected abstract void OnPursuitStarted(LHandle pursuitHandle);

        /// <summary>
        /// Event that is being invoked when a pursuit is ended in the game world.
        /// </summary>
        /// <param name="pursuitHandle">The LSPDFR pursuit handle.</param>
        protected abstract void OnPursuitEnded(LHandle pursuitHandle);
    }
}