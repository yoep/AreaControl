using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Managers;
using AreaControl.Utils;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadImpl : ICloseRoad
    {
        private const float CarSize = 5.5f;
        private const float ScanRadius = 250f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;

        public CloseRoadImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager)
        {
            _rage = rage;
            _entityManager = entityManager;
            _responseManager = responseManager;
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public UIMenuItem Item { get; } = new UIMenuItem("CloseRoad_Placeholder");
        
        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation()
        {
            _rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE " + _responseManager.ResponseCodeAudio);
                var blockSlots = DetermineBlockSlots();
                SpawnBlockSlots(blockSlots);
            }, "AreaControl.CloseRoad");
        }

        #endregion

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
            closestRoad.CreatePreview();

            for (var i = 0; i < closestRoad.Width / CarSize; i++)
            {
                blockSlots.Add(new BlockSlot(placementPosition, placementHeading));
                placementPosition = placementPosition + direction * CarSize;
            }

            return blockSlots;
        }

        private void SpawnBlockSlots(IEnumerable<BlockSlot> blockSlots)
        {
            var i = 0;
            
            foreach (var slot in blockSlots)
            {
                i++;
                _rage.NewSafeFiber(() =>
                {
                    slot.CreatePreview();
                    
                    var vehicle = _entityManager.FindVehicleWithinOrCreate(slot.Position, ScanRadius);
                    MoveToSlot(vehicle, slot);
                    AssignRedirectTrafficDuty(vehicle.Passengers.First(), slot);

                    while (true)
                    {
                        GameFiber.Yield();
                    }
                }, "BlockSlot#" + i);
            }
        }

        private void MoveToSlot(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleDriver = vehicle.Driver;

            _rage.LogTrivialDebug("Vehicle driving to block slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 30f, VehicleDrivingFlags.Normal, 35f)
                .WaitForCompletion();
            _rage.LogTrivialDebug("Vehicle arrived in the area of block slot " + slot);
            vehicle.Instance.IsSirenOn = true;
            vehicle.Instance.IsSirenSilent = true;
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 10f, VehicleDrivingFlags.Emergency, 2f)
                .WaitForCompletion(10000);
            vehicle.Instance.Heading = slot.Heading;
            _rage.LogTrivialDebug("Vehicle parked at block slot " + slot);

            vehicle.Empty();
        }

        private void AssignRedirectTrafficDuty(ACPed ped, BlockSlot slot)
        {
            ped.ActivateDuty(new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading));
        }
    }
}