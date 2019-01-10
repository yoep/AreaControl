using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class RemoveCloseRoadPreview : IMenuComponent
    {
        private readonly ICloseRoadPreview _roadPreview;

        public RemoveCloseRoadPreview(ICloseRoadPreview roadPreview)
        {
            _roadPreview = roadPreview;
        }

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionRemoveCloseRoadPreview);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => _roadPreview.IsActive;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            _roadPreview.OpenRoad();
        }
    }
}