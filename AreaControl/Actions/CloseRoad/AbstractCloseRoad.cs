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
        private const float DistanceFromPlayer = 25f;

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

        #endregion

        protected ICollection<BlockSlot> DetermineBlockSlots()
        {
            var closestRoadToPlayer = DetermineClosestRoadTo(Game.LocalPlayer.Character.Position);
            var blockSlots = new List<BlockSlot>();

            foreach (var road in GetRoadsAwayFromPlayer(closestRoadToPlayer))
            {
                var placementHeading = road.Lanes.First().Heading + 90f;
                var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
                var placementPosition = road.Lanes.First().RightSide + direction * 2f;
                Rage.LogTrivialDebug("Found road to use " + road);

                for (var i = 0; i < road.Width / CarSize; i++)
                {
                    blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                    placementPosition = placementPosition + direction * CarSize;
                }

                Rage.LogTrivialDebug("Heading in regards to player: " + MathHelper.ConvertDirectionToHeading((Game.LocalPlayer.Character.Position - road.Position)));
            }

            Rage.LogTrivialDebug("Created " + blockSlots.Count + " block slot(s)");
            return blockSlots;
        }

        protected Road DetermineClosestRoadTo(Vector3 position)
        {
            return RoadUtil.GetClosestRoad(position, RoadType.All);
        }

        private IEnumerable<Road> GetRoadsAwayFromPlayer(Road closestRoadToPlayer)
        {
            var originalHeading = closestRoadToPlayer.Lanes.First().Heading;
            var oppositeHeading = -originalHeading;
            var originalDirection = MathHelper.ConvertHeadingToDirection(originalHeading);
            var oppositeDirection = MathHelper.ConvertHeadingToDirection(oppositeHeading);

            return new List<Road>
            {
                DetermineClosestRoadTo(closestRoadToPlayer.Position + originalDirection * DistanceFromPlayer),
                DetermineClosestRoadTo(closestRoadToPlayer.Position + oppositeDirection * DistanceFromPlayer)
            };
        }
    }
}