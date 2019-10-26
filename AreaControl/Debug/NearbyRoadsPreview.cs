using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils.Road;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class NearbyRoadsPreview : INearbyRoadsPreview
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;

        private List<Road> _roads;

        #region Constructors

        public NearbyRoadsPreview(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
        }

        #endregion

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.NearbyRoadsPreview);

        /// <inheritdoc />
        public MenuType Type => MenuType.DEBUG;

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (_roads != null)
            {
                RemoveRoadsPreview();
            }
            else
            {
                CreateRoadsPreview();
            }
        }

        #endregion

        private void CreateRoadsPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.NearbyRoadsPreviewRemove;
                _roads = RoadUtils.GetNearbyRoads(Game.LocalPlayer.Character.Position, RoadType.All).ToList();
                _logger.Debug("--- NEARBY ROADS ---");
                _roads.ForEach(x =>
                {
                    _logger.Debug(x.ToString());
                    x.CreatePreview();
                });
            });
        }

        private void RemoveRoadsPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.NearbyRoadsPreview;
                _roads.ForEach(x => x.DeletePreview());
            });
        }
    }
}