using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public abstract class AbstractCloseRoad : ICloseRoad
    {
        private const float CarSize = 5.5f;
        private const float DistanceFromPlayer = 8f;

        protected readonly IRage Rage;

        #region Constructors

        protected AbstractCloseRoad(IRage rage)
        {
            Rage = rage;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public abstract UIMenuItem MenuItem { get; }

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public abstract bool IsVisible { get; }

        /// <inheritdoc />
        public abstract void OnMenuActivation(IMenu sender);

        #endregion

        #region ICloseRoad implementation

        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        /// <inheritdoc />
        public abstract void OpenRoad();

        #endregion

        protected ICollection<BlockSlot> DetermineBlockSlots()
        {
            var closestRoad = DetermineClosestRoad();
            var placementHeading = closestRoad.Lanes.First().Heading + 90f;
            var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
            var placementPosition = closestRoad.Lanes.First().RightSide + direction * 2f;
            var blockSlots = new List<BlockSlot>();
            Rage.LogTrivialDebug("Found road to use " + closestRoad);

            for (var i = 0; i < closestRoad.Width / CarSize; i++)
            {
                blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                placementPosition = placementPosition + direction * CarSize;
            }

            Rage.LogTrivialDebug("Created " + blockSlots.Count + " block slot(s)");
            return blockSlots;
        }

        protected Road DetermineClosestRoad()
        {
            var closestRoad = RoadUtil.GetClosestRoad(Game.LocalPlayer.Character.Position, RoadType.All);
            Rage.LogTrivialDebug("Found road to use " + closestRoad);

            return closestRoad;
        }
    }
}