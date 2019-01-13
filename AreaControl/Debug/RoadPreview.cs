using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Debug
{
    public class RoadPreview : IMenuComponent
    {
        private Road _road;

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RoadPreview);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => true;

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
            MenuItem.Text = AreaControl.RoadPreviewRemove;
            _road = RoadUtil.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            _road.CreatePreview();
        }

        private void RemoveRoadPreview()
        {
            MenuItem.Text = AreaControl.RoadPreview;
            _road.DeletePreview();
            _road = null;
        }
    }
}