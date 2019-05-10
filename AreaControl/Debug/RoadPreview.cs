using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class RoadPreview : IMenuComponent
    {
        private readonly IRage _rage;
        private Road _road;

        public RoadPreview(IRage rage)
        {
            _rage = rage;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RoadPreview);

        /// <inheritdoc />
        public bool IsAutoClosed => false;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public bool IsDebug => true;

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

        private void CreateRoadPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.RoadPreviewRemove;
                _road = RoadUtil.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
                _road.CreatePreview();
            }, "RoadPreview.CreateRoadPreview");
        }

        private void RemoveRoadPreview()
        {
            _rage.NewSafeFiber(() =>
            {
                MenuItem.Text = AreaControl.RoadPreview;
                _road.DeletePreview();
                _road = null;
            }, "RoadPreview.RemoveRoadPreview");
        }
    }
}