using RAGENativeUI.Elements;

namespace AreaControl.Actions.RoadBlock
{
    public class RoadBlockImpl : IRoadBlock
    {
        /// <inheritdoc />
        public UIMenuItem Item { get; } = new UIMenuItem("RoadBlock_Placeholder");
        
        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation()
        {
            //TODO: implement
        }
    }
}