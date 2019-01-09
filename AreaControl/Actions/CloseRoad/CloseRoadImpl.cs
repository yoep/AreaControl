using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    [IoC.Primary]
    public class CloseRoadImpl : AbstractCloseRoad
    {
        private const float ScanRadius = 250f;

        private readonly IList<IDuty> _duties = new List<IDuty>();
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;

        public CloseRoadImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager)
            : base(rage)
        {
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
        }

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem Item { get; } = new UIMenuItem(AreaControl.ActionCloseRoad);

        /// <inheritdoc />
        public override bool IsVisible => !IsActive;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            IsActive = true;
            sender.ReplaceComponent(this, IoC.Instance.GetInstance<IOpenRoad>());
            Rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE " + _responseManager.ResponseCodeAudio);
                var blockSlots = DetermineBlockSlots();
                SpawnBlockSlots(blockSlots);
            }, "AreaControl.CloseRoad");
        }

        #endregion

        #region ICloseRoad implementation

        /// <inheritdoc />
        public override void OpenRoad()
        {
            foreach (var duty in _duties)
            {
                duty.Abort();
            }

            IsActive = false;
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
                        //get position behind the slot
                        var positionBehindSlot = GetPositionBehindSlot(slot);
                        var vehicle = _entityManager.FindVehicleWithinOrCreateAt(slot.Position, positionBehindSlot.Position, ScanRadius);
                        MoveToSlot(vehicle, slot);

                        AssignRedirectTrafficDuty(vehicle, slot);
                        var nextAvailableDuty = _dutyManager.GetNextAvailableDuty(Game.LocalPlayer.Character.Position);

                        if (nextAvailableDuty != null)
                            vehicle.Passengers.First().ActivateDuty(nextAvailableDuty);
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
            WarpVehicleInHeading(vehicle, slot);
            Rage.LogTrivialDebug("Vehicle parked at block slot " + slot);

            var emptyVehicleTask = vehicle.Empty()
                .WaitForCompletion(10000);
            Rage.LogTrivialDebug("Empty vehicle task ended with " + emptyVehicleTask);
        }

        private static void WarpVehicleInHeading(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleHeading = vehicle.Instance.Heading;
            var expectedHeading = slot.Heading;

            if (Math.Abs(vehicleHeading - expectedHeading) > 20f)
                vehicle.Instance.Heading = expectedHeading;
        }

        private void AssignRedirectTrafficDuty(ACVehicle vehicle, BlockSlot slot)
        {
            var trafficDuty = new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading);
            _duties.Add(trafficDuty);
            vehicle.Driver.ActivateDuty(trafficDuty);
        }

        private static Road GetPositionBehindSlot(BlockSlot slot)
        {
            return RoadUtil.GetClosestRoad(slot.Position + MathHelper.ConvertHeadingToDirection(slot.PedHeading) * 60f, RoadType.All);
        }
    }
}