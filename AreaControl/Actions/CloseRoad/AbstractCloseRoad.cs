using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public abstract class AbstractCloseRoad : ICloseRoad
    {
        private const float CarSize = 5.5f;

        protected readonly IRage Rage;

        #region Constructors

        protected AbstractCloseRoad(IRage rage)
        {
            Rage = rage;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public abstract UIMenuItem Item { get; }

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public abstract void OnMenuActivation();

        #endregion

        protected IEnumerable<BlockSlot> DetermineBlockSlots()
        {
            var player = Game.LocalPlayer.Character;
            var playerPosition = player.Position;
            var closestRoad = RoadUtil.GetClosestRoad(playerPosition, RoadType.All);
            var placementHeading = closestRoad.Heading + 90f;
            var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
            var placementPosition = closestRoad.RightSide + direction * 2f;
            var blockSlots = new List<BlockSlot>();
            Rage.LogTrivialDebug("Found road to use " + closestRoad);
            closestRoad.CreatePreview();

            for (var i = 0; i < closestRoad.Width / CarSize; i++)
            {
                blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                placementPosition = placementPosition + direction * CarSize;
            }

            return blockSlots;
        }
    }
}