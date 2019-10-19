using Rage;

namespace AreaControl.AbstractionLayer.Implementation
{
    public class GameFiberWrapper : IGameFiberWrapper
    {
        private readonly GameFiber _gameFiber;

        public GameFiberWrapper(GameFiber gameFiber)
        {
            _gameFiber = gameFiber;
        }

        /// <inheritdoc />
        public string Name => _gameFiber.Name;

        /// <inheritdoc />
        public bool IsAlive => _gameFiber.IsAlive;

        /// <inheritdoc />
        public void Abort()
        {
            _gameFiber.Abort();
        }
    }
}