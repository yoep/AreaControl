using System.Collections.Generic;
using AreaControl.Managers;
using AreaControl.Model;
using AreaControl.Rage;
using AreaControl.Utils;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadImpl : ICloseRoad
    {
        private const float CarSize = 5.5f;
        private const float ScanRadius = 250f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;

        public CloseRoadImpl(IRage rage, IEntityManager entityManager)
        {
            _rage = rage;
            _entityManager = entityManager;
        }

        /// <inheritdoc />
        public void OnMenuActivation()
        {
            GameFiber.StartNew(() =>
            {
                Functions.PlayScannerAudio("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE UNITS_RESPOND_CODE_02");
                var blockSlots = DetermineBlockSlots();
                SpawnBlockSlots(blockSlots);
            }, "AreaControl.CloseRoad");
        }

        private IEnumerable<BlockSlot> DetermineBlockSlots()
        {
            var player = Game.LocalPlayer.Character;
            var playerPosition = player.Position;
            var closestRoad = RoadUtil.GetClosestRoad(playerPosition, RoadType.All);
            var placementHeading = closestRoad.Heading + 90f;
            var direction = MathHelper.ConvertHeadingToDirection(placementHeading);
            var placementPosition = closestRoad.RightSide + direction * 2f;
            var blockSlots = new List<BlockSlot>();
            _rage.LogTrivialDebug("Found road to use " + closestRoad);

            for (var i = 0; i < closestRoad.Width / CarSize; i++)
            {
                blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                placementPosition = placementPosition + direction * CarSize;
            }

            return blockSlots;
        }

        private void SpawnBlockSlots(IEnumerable<BlockSlot> blockSlots)
        {
            foreach (var slot in blockSlots)
            {
                GameFiber.StartNew(() =>
                {
                    var vehicle = _entityManager.FindVehicleWithinOrCreate(slot.Position, ScanRadius);
                    MoveToSlot(vehicle, slot);

                    while (true)
                    {
                        GameFiber.Yield();
                    }
                });
            }
        }

        private void MoveToSlot(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleDriver = vehicle.Driver;

            slot.CreatePreview();
            _rage.LogTrivialDebug("Vehicle driving to block slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 30f, VehicleDrivingFlags.Normal, 30f)
                .WaitForCompletion();
            _rage.LogTrivialDebug("Vehicle arrived in the area of block slot " + slot);
            vehicle.Instance.IsSirenOn = true;
            vehicle.Instance.IsSirenSilent = true;
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 10f, VehicleDrivingFlags.Emergency, 3f)
                .WaitForCompletion();
            _rage.LogTrivialDebug("Vehicle parked at block slot " + slot);
            vehicle.Instance.Position = slot.Position;
            vehicle.Instance.Heading = slot.Heading;
        }
    }
}