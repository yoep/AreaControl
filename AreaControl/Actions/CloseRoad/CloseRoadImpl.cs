using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Managers;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    public class CloseRoadImpl : AbstractCloseRoad
    {
        private const float ScanRadius = 250f;

        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;

        public CloseRoadImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager)
            : base(rage)
        {
            _entityManager = entityManager;
            _responseManager = responseManager;
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem Item { get; } = new UIMenuItem("CloseRoad_Placeholder");

        /// <inheritdoc />
        public override void OnMenuActivation()
        {
            Rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE " + _responseManager.ResponseCodeAudio);
                var blockSlots = DetermineBlockSlots();
                SpawnBlockSlots(blockSlots);
            }, "AreaControl.CloseRoad");
        }

        #endregion

        private void SpawnBlockSlots(IEnumerable<BlockSlot> blockSlots)
        {
            var i = 0;

            foreach (var slot in blockSlots)
            {
                i++;
                Rage.NewSafeFiber(() =>
                    {
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
            var initialDrivingFlags = _responseManager.ResponseCode == ResponseCode.Code2 ? VehicleDrivingFlags.Normal : VehicleDrivingFlags.Emergency;
            var initialDrivingSpeed = _responseManager.ResponseCode == ResponseCode.Code2 ? 30f : 45f;

            if (_responseManager.ResponseCode == ResponseCode.Code3)
                vehicle.EnableSirens();

            Rage.LogTrivialDebug("Vehicle driving to block slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, initialDrivingSpeed, initialDrivingFlags, 35f)
                .WaitForCompletion();
            Rage.LogTrivialDebug("Vehicle arrived in the area of block slot " + slot);
            vehicle.EnableEmergencyLights();
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 10f, VehicleDrivingFlags.Emergency, 2f)
                .WaitForCompletion(20000);
            vehicle.Instance.Heading = slot.Heading;
            Rage.LogTrivialDebug("Vehicle parked at block slot " + slot);

            vehicle.Empty();
        }

        private void AssignRedirectTrafficDuty(ACPed ped, BlockSlot slot)
        {
            ped.ActivateDuty(new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading));
        }
    }
}