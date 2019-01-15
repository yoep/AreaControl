using AreaControl.Menu;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RoadBlock
{
    public class RoadBlockImpl : IRoadBlock
    {
        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem("RoadBlock_Placeholder");
        
        /// <inheritdoc />
        public bool IsAutoClosed => true;

        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            //TODO: implement
        }
        
        /// <inheritdoc />
        public void OnMenuHighlighted(IMenu sender)
        {
            //do nothing
        }
    }
}