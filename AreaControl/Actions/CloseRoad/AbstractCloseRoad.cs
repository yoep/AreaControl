using System;
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
        private const float DistanceFromPlayer = 25f;
        private const float LaneHeadingTolerance = 40f;

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

        /// <inheritdoc />
        public virtual void OnMenuHighlighted(IMenu sender)
        {
            //do nothing
        }

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
                var roadHeadingToOriginal =
                    MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(closestRoadToPlayer.Position - road.Position));
                Rage.LogTrivialDebug("Found road to use " + road);
                Rage.LogTrivialDebug("Road heading in regards to closest road " + roadHeadingToOriginal);
                var lanesToBlock = road.Lanes
                    .Where(x => Math.Abs(x.Heading - roadHeadingToOriginal) < LaneHeadingTolerance)
                    .ToList();
                Rage.LogTrivialDebug("Found " + lanesToBlock.Count + " lanes to block");

                if (!lanesToBlock.Any())
                {
                    lanesToBlock = road.Lanes.ToList();
                    Rage.LogTrivialDebug("Using all lanes as no lanes were found to be used");
                }

                foreach (var lane in lanesToBlock)
                {
                    var placementHeading = lane.Heading + 90f;
                    var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
                    var placementPosition = lane.RightSide + direction * 2f;

                    blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                }
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
            var roads = new List<Road> {DetermineClosestRoadTo(closestRoadToPlayer.Position + oppositeDirection * DistanceFromPlayer)};

            // only add an additional block if the road is not a single direction road
            if (!closestRoadToPlayer.IsSingleDirection)
                roads.Add(DetermineClosestRoadTo(closestRoadToPlayer.Position + originalDirection * DistanceFromPlayer));

            return roads;
        }
    }
}