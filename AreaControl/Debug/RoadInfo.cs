using AreaControl.AbstractionLayer;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class RoadInfo : IMenuComponent
    {
        private readonly IRage _rage;

        public RoadInfo(IRage rage)
        {
            _rage = rage;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RoadInfo);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public bool IsDebug => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            var road = RoadUtil.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            _rage.LogTrivial("Nearest road info: " + road);
            _rage.DisplayPluginNotification("see console or log file for info about the closest road");
        }
        
        /// <inheritdoc />
        public void OnMenuHighlighted(IMenu sender)
        {
            //do nothing
        }
    }
}