using System;
using System.Collections.Generic;
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
        private const float BlockHeadingTolerance = 30f;
        private const float BlockPositionTolerance = 8f;

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
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionCloseRoad);

        /// <inheritdoc />
        public override bool IsVisible => !IsActive;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            IsActive = true;
            sender.ReplaceComponent(this, new OpenRoad(this));
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
            _dutyManager.DismissDuties();
            IsActive = false;
        }

        #endregion

        private void SpawnBlockSlots(ICollection<BlockSlot> blockSlots)
        {
            var i = 0;

            Rage.LogTrivialDebug("Spawning " + blockSlots.Count + " block slot(s) for close road function");
            foreach (var slot in blockSlots)
            {
                i++;
                var index = i;
                Rage.NewSafeFiber(() =>
                    {
                        //get position behind the slot
                        var positionBehindSlot = GetPositionBehindSlot(slot, index);
                        var vehicle = _entityManager.FindVehicleWithinOrCreateAt(slot.Position, positionBehindSlot.Position, ScanRadius);
                        Rage.LogTrivialDebug("Using vehicle " + vehicle + " for block slot " + index);
                        
                        MoveToSlot(vehicle, slot);
                        AssignRedirectTrafficDutyToDriver(vehicle, slot);
                        AssignAvailableDutiesToPassengers(vehicle, slot);
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
            WarpVehicleInPosition(vehicle, slot);
            WarpVehicleInHeading(vehicle, slot);
            Rage.LogTrivialDebug("Vehicle parked at block slot " + slot);

            var emptyVehicleTask = vehicle.Empty()
                .WaitForCompletion(10000);
            Rage.LogTrivialDebug("Empty vehicle task ended with " + emptyVehicleTask);
        }

        private void WarpVehicleInHeading(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleHeading = vehicle.Instance.Heading;
            var expectedHeading = slot.Heading;
            var headingDifference = Math.Abs(vehicleHeading - expectedHeading);

            Rage.LogTrivialDebug("Checking heading tolerance, expected: " + expectedHeading + ", actual: " + vehicleHeading + ", difference: " +
                                 headingDifference);
            if (headingDifference > BlockHeadingTolerance)
                vehicle.Instance.Heading = expectedHeading;
        }

        private void WarpVehicleInPosition(ACVehicle vehicle, BlockSlot slot)
        {
            var vehiclePosition = vehicle.Instance.Position;
            var expectedPosition = slot.Position;
            var positionDifference = Vector3.Distance(vehiclePosition, expectedPosition);

            Rage.LogTrivialDebug("Checking position tolerance, expected: " + expectedPosition + ", actual: " + vehiclePosition + ", difference: " +
                                 positionDifference);
            if (positionDifference > BlockPositionTolerance)
                vehicle.Instance.Position = expectedPosition;
        }

        private void AssignRedirectTrafficDutyToDriver(ACVehicle vehicle, BlockSlot slot)
        {
            var trafficDuty = new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading);
            _dutyManager.RegisterDuty(trafficDuty);
            vehicle.Driver.ActivateDuty(trafficDuty);
        }

        private void AssignAvailableDutiesToPassengers(ACVehicle vehicle, BlockSlot slot)
        {
            vehicle.Passengers.ForEach(AssignNextAvailableDutyToPed);
        }

        private void AssignNextAvailableDutyToPed(ACPed ped)
        {
            var nextAvailableDuty = _dutyManager.NextAvailableOrIdleDuty(Game.LocalPlayer.Character.Position);

            if (nextAvailableDuty.GetType() != typeof(ReturnToVehicleDuty))
                nextAvailableDuty.OnCompletion += (sender, args) => AssignNextAvailableDutyToPed(ped);

            ped.ActivateDuty(nextAvailableDuty);
        }

        private static Road GetPositionBehindSlot(BlockSlot slot, int index)
        {
            return RoadUtil.GetClosestRoad(slot.Position + MathHelper.ConvertHeadingToDirection(slot.PedHeading) * (80f * index), RoadType.All);
        }
    }
}