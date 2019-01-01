using AreaControl.Rage;
using AreaControl.Utils;
using Rage;

namespace AreaControl.CloseRoad
{
    public class CloseRoadImpl : ICloseRoad
    {
        private const float CarSize = 5.5f;

        private readonly IRage _rage;
        private readonly IRoadUtil _roadUtil;

        public CloseRoadImpl(IRage rage, IRoadUtil roadUtil)
        {
            _rage = rage;
            _roadUtil = roadUtil;
        }

        public void OnMenuActivation()
        {
            var player = Game.LocalPlayer.Character;
            var playerPosition = player.Position;
            var closestRoad = _roadUtil.GetClosestRoad(playerPosition, RoadType.All);
            var placementHeading = closestRoad.Heading + 90f;
            var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
            var placementPosition = closestRoad.RightSide + direction * 2f;
            _rage.LogTrivialDebug("Found road to use " + closestRoad);

            for (var i = 0; i < closestRoad.Width / CarSize; i++)
            {
                new Vehicle("POLICE", placementPosition, placementHeading) {IsPersistent = true, IsEngineOn = true, IsSirenOn = true, IsSirenSilent = true};
                placementPosition = placementPosition + direction * CarSize;
            }
        }
    }
}