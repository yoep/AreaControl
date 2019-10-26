using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class RoadPreview : IRoadPreview
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private Road _road;

        public RoadPreview(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RoadPreview);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_road == null)
            {
                CreateRoadPreview();
            }
            else
            {
                RemoveRoadPreview();
            }
        }

        #endregion

        #region IRoadPreview

        /// <inheritdoc />
        public void ShowRoadPreview()
        {
            if (_road == null)
                CreateRoadPreview();
        }

        /// <inheritdoc />
        public void HideRoadPreview()
        {
            if (_road != null)
                RemoveRoadPreview();
        }

        #endregion

        private void CreateRoadPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.RoadPreviewRemove;
                _road = RoadUtils.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
                _logger.Debug("Nearest road info: " + _road);
                _road.CreatePreview();
            });
        }

        private void RemoveRoadPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.RoadPreview;
                _road.DeletePreview();
                _road = null;
            });
        }
    }
}