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
        private const float DistanceFromPlayer = 15f;
        private const float LaneHeadingTolerance = 40f;

        protected readonly IRage Rage;
        protected readonly ILogger Logger;

        #region Constructors

        protected AbstractCloseRoad(IRage rage, ILogger logger)
        {
            Rage = rage;
            Logger = logger;
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
        public abstract bool IsDebug { get; }

        /// <inheritdoc />
        public abstract void OnMenuActivation(IMenu sender);

        #endregion

        #region ICloseRoad implementation

        /// <inheritdoc />
        public bool IsActive { get; protected set; }

        #endregion

        protected ICollection<BlockSlot> DetermineBlockSlots()
        {
            var blockSlots = new List<BlockSlot>();
            var closestRoadToPlayer = DetermineClosestRoadTo(Game.LocalPlayer.Character.Position);
            var roads = closestRoadToPlayer.IsSingleDirection
                ? GetRoadBehindGivenRoad(closestRoadToPlayer)
                : GetRoadsAwayFromPlayer(closestRoadToPlayer);

            foreach (var road in roads)
            {
                var roadHeadingToOriginal =
                    MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(closestRoadToPlayer.Position - road.Position));
                Logger.Debug("Found road to use " + road);
                Logger.Debug("Road heading in regards to closest road " + roadHeadingToOriginal);
                var lanesToBlock = road.Lanes
                    .Where(x => Math.Abs(x.Heading - roadHeadingToOriginal) < LaneHeadingTolerance)
                    .ToList();
                Logger.Debug("Found " + lanesToBlock.Count + " lanes to block");

                foreach (var lane in lanesToBlock)
                {
                    var laneRightSide = lane.RightSide + MathHelper.ConvertHeadingToDirection(lane.Heading) * DistanceFromPlayer;
                    var placementPosition = laneRightSide + MathHelper.ConvertHeadingToDirection(lane.Heading + 90f) * 2f;

                    blockSlots.Add(new BlockSlot(placementPosition, lane.Heading));
                }
            }

            Logger.Debug("Created " + blockSlots.Count + " block slot(s)");
            return blockSlots;
        }

        protected Road DetermineClosestRoadTo(Vector3 position)
        {
            return RoadUtil.GetClosestRoad(position, RoadType.All);
        }

        private IEnumerable<Road> GetRoadsAwayFromPlayer(Road closestRoadToPlayer)
        {
            var originalHeading = closestRoadToPlayer.Lanes.First().Heading;
            var oppositeHeading = RoadUtil.OppositeHeading(originalHeading);
            var originalDirection = MathHelper.ConvertHeadingToDirection(originalHeading);
            var oppositeDirection = MathHelper.ConvertHeadingToDirection(oppositeHeading);

            return new List<Road>
            {
                DetermineClosestRoadTo(closestRoadToPlayer.Position + oppositeDirection * DistanceFromPlayer),
                DetermineClosestRoadTo(closestRoadToPlayer.Position + originalDirection * DistanceFromPlayer)
            };
        }

        private IEnumerable<Road> GetRoadBehindGivenRoad(Road closestRoadToPlayer)
        {
            var position = Game.LocalPlayer.Character.Position;
            var oppositeHeading = RoadUtil.OppositeHeading(RoadUtil.GetClosestLane(closestRoadToPlayer, position).Heading);
            var oppositeDirection = MathHelper.ConvertHeadingToDirection(oppositeHeading);

            return new List<Road>
            {
                DetermineClosestRoadTo(closestRoadToPlayer.Position + oppositeDirection * DistanceFromPlayer)
            };
        }
    }
}