using AreaControl.AbstractionLayer;
using AreaControl.Menu;
using AreaControl.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class RoadInfo : IRoadInfo
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;

        public RoadInfo(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RoadInfo);
        
        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var road = RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            _logger.Info("Nearest road info: " + road);
            _rage.DisplayPluginNotification("see console or log file for info about the closest road");
        }
    }
}