namespace AreaControl.AbstractionLayer
{
    public interface IGameFiberWrapper
    {
        /// <summary>
        /// Gets the name of this instance.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets a value indicating whether this instance is still running.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Aborts this instance.
        /// </summary>
        void Abort();
    }
}