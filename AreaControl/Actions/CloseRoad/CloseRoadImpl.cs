using System.Collections.Generic;
using AreaControl.Rage;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadImpl : ICloseRoad
    {
        private const float CarSize = 5.5f;
        private const float ScanRadius = 250f;

        private readonly IRage _rage;
        private readonly IRoadUtil _roadUtil;
        private readonly IVehicleQuery _vehicleQuery;

        public CloseRoadImpl(IRage rage, IRoadUtil roadUtil, IVehicleQuery vehicleQuery)
        {
            _rage = rage;
            _roadUtil = roadUtil;
            _vehicleQuery = vehicleQuery;
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
            var closestRoad = _roadUtil.GetClosestRoad(playerPosition, RoadType.All);
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
                var vehicle = _vehicleQuery.FindWithin(slot.Position, ScanRadius);

                if (vehicle == null)
                    vehicle = Functions.RequestBackup(slot.Position, EBackupResponseType.Code2, EBackupUnitType.LocalUnit);

                GameFiber.StartNew(() =>
                {
                    ClaimVehicleOccupants(vehicle);
                    MoveToSlot(vehicle, slot);

                    //keep claiming the vehicle
                    while (true)
                    {
                        GameFiber.Yield();
                    }
                });

                GameFiber.Sleep(100);
            }
        }

        private void MoveToSlot(Vehicle vehicle, BlockSlot slot)
        {
            var vehicleDriver = vehicle.Driver;
            
            slot.CreatePreview();
            _rage.LogTrivialDebug("Vehicle driving to block slot...");
            vehicleDriver.Tasks
                .DriveToPosition(slot.Position, 30f, VehicleDrivingFlags.Normal, 5f)
                .WaitForCompletion();
            _rage.LogTrivialDebug("Vehicle arrived at block slot " + slot);
            vehicle.Position = slot.Position;
            vehicle.Heading = slot.Heading;
            vehicle.IsSirenOn = true;
            vehicle.IsSirenSilent = true;
        }

        private void ClaimVehicleOccupants(Vehicle vehicle)
        {
            foreach (var vehicleOccupant in vehicle.Occupants)
            {
                Functions.SetCopAsBusy(vehicleOccupant, true);
            }
        }
    }
}