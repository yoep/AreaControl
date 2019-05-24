using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using AreaControl.Settings;
using AreaControl.Utils;
using AreaControl.Utils.Road;
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
        private readonly ISettingsManager _settingsManager;

        private ICollection<BlockSlot> _blockSlots;

        #region Constructors

        public CloseRoadImpl(IRage rage, ILogger logger, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager,
            ISettingsManager settingsManager)
            : base(rage, logger)
        {
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
            _settingsManager = settingsManager;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ActionCloseRoad, AreaControl.ActionCloseRoadDescription);

        /// <inheritdoc />
        public override MenuType Type => MenuType.STREET_CONTROL;

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                OpenRoad();
            }
            else
            {
                CloseRoad();
            }
        }

        #endregion

        #region Functions

        private void OpenRoad()
        {
            MenuItem.Text = AreaControl.ActionCloseRoad;
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            _dutyManager.DismissDuties();
            _entityManager.Dismiss();
            ClearBarriers();
            IsActive = false;
        }

        private void CloseRoad()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.ActionOpenRoad;
            Rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                Rage.DisplayNotification("Requesting dispatch to ~b~close nearby road~s~ " + World.GetStreetName(position) + "...");
                Functions.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION " + _responseManager.ResponseCodeAudio,
                    position);
                _blockSlots = DetermineBlockSlots();

                GameFiber.Sleep(5000);
                if (_blockSlots.Count > 0)
                {
                    SpawnBlockSlots(_blockSlots);
                }
                else
                {
                    Logger.Warn("Unable to create any road block slots");
                }
            }, "AreaControl.CloseRoad");
        }

        private void ClearBarriers()
        {
            try
            {
                foreach (var barriers in _blockSlots.Select(x => x.Barriers))
                {
                    foreach (var barrier in barriers)
                    {
                        PropUtils.Remove(barrier.Object.Instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while clearing barriers with error: {ex.Message}", ex);
            }
        }

        private void SpawnBlockSlots(ICollection<BlockSlot> blockSlots)
        {
            var i = 0;

            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");
            Logger.Debug($"Spawning {blockSlots.Count} block slot(s) for close road function");
            foreach (var slot in blockSlots)
            {
                i++;
                var index = i;
                Rage.NewSafeFiber(() =>
                    {
                        //get position behind the slot
                        var positionBehindSlot = GetPositionBehindSlot(slot, index);
                        var vehicle = _entityManager.FindVehicleWithinOrCreateAt(slot.Position, positionBehindSlot.Position, ScanRadius, 2);
                        vehicle.SetOccupantsBusyState(true);
                        Logger.Debug($"Using vehicle {vehicle} for block slot {index}");

                        MoveToSlot(vehicle, slot);
                        AssignDutiesToDriver(vehicle.Driver, slot);
                        AssignAvailableDutiesToPassengers(vehicle, slot);
                    }, "BlockSlot#" + i);
            }
        }

        private void MoveToSlot(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleDriver = vehicle.Driver;
            var initialDrivingFlags = _responseManager.VehicleDrivingFlags;
            var initialDrivingSpeed = _responseManager.VehicleSpeed;

            if (_responseManager.ResponseCode == ResponseCode.Code3)
                vehicle.EnableSirens();

            Logger.Trace("Vehicle driving to block slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, initialDrivingSpeed, initialDrivingFlags, 35f)
                .WaitForCompletion();
            Logger.Trace("Vehicle arrived in the area of block slot " + slot);
            slot.ClearSlotFromTraffic();
            vehicle.EnableEmergencyLights();
            vehicleDriver.Instance.Tasks
                .DriveToPosition(slot.Position, 10f, VehicleDrivingFlags.Emergency, 2f)
                .WaitForCompletion(20000);
            WarpVehicleInPosition(vehicle, slot);
            WarpVehicleInHeading(vehicle, slot);
            Logger.Trace("Vehicle parked at block slot " + slot);
        }

        private void WarpVehicleInHeading(ACVehicle vehicle, BlockSlot slot)
        {
            var vehicleHeading = vehicle.Instance.Heading;
            var expectedHeading = slot.Heading;
            var headingDifference = Math.Abs(vehicleHeading - expectedHeading);

            Logger.Debug("Checking heading tolerance, expected: " + expectedHeading + ", actual: " + vehicleHeading + ", difference: " +
                         headingDifference);
            if (headingDifference > BlockHeadingTolerance)
                vehicle.Instance.Heading = expectedHeading;
        }

        private void WarpVehicleInPosition(ACVehicle vehicle, BlockSlot slot)
        {
            var vehiclePosition = vehicle.Instance.Position;
            var expectedPosition = slot.Position;
            var positionDifference = Vector3.Distance(vehiclePosition, expectedPosition);

            Logger.Debug("Checking position tolerance, expected: " + expectedPosition + ", actual: " + vehiclePosition + ", difference: " +
                         positionDifference);
            if (positionDifference > BlockPositionTolerance)
                vehicle.Instance.Position = expectedPosition;
        }

        private void AssignDutiesToDriver(ACPed ped, BlockSlot slot)
        {
            ped.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
            AssignPlaceBarriersDuty(ped, slot);
            _dutyManager.RegisterDuty(ped, new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading, _responseManager.ResponseCode));
        }

        private void AssignAvailableDutiesToPassengers(ACVehicle vehicle, BlockSlot slot)
        {
            var passengers = vehicle.Passengers;
            passengers.ForEach(x =>
            {
                x.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                AssignNextAvailableDutyToPed(x);
            });
        }

        private void AssignPlaceBarriersDuty(ACPed ped, BlockSlot slot)
        {
            if (!_settingsManager.CloseRoadSettings.PlaceBarriers)
                return;

            var objects = slot.Barriers
                .Select(barrier => barrier.Object)
                .ToList();

            var placeObjectsDuty = new PlaceObjectsDuty(_dutyManager.GetNextDutyId(), objects, _responseManager.ResponseCode, false);
            Logger.Debug("Created place barriers duty " + placeObjectsDuty);
            _dutyManager.RegisterDuty(ped, placeObjectsDuty);
        }

        private void AssignNextAvailableDutyToPed(ACPed ped)
        {
            var nextAvailableDuty = _dutyManager.NextAvailableOrIdleDuty(ped, GetDutyTypes());

            if (nextAvailableDuty.GetType() != typeof(ReturnToVehicleDuty))
            {
                ActivateNonIdleDuty(ped, nextAvailableDuty);
            }
            else
            {
                nextAvailableDuty.OnCompletion += (sender, args) => RegisterDutyListenerForIdlePed(ped);
            }
        }

        private void ActivateNonIdleDuty(ACPed ped, IDuty duty)
        {
            duty.OnCompletion += (sender, args) => AssignNextAvailableDutyToPed(ped);
        }

        private void RegisterDutyListenerForIdlePed(ACPed ped)
        {
            var dutyListener = _dutyManager[ped];
            dutyListener.DutyTypes.Add(DutyType.CleanCorpses);
            dutyListener.DutyTypes.Add(DutyType.CleanWrecks);
            dutyListener.OnDutyAvailable += (sender, args) => ActivateNonIdleDuty(ped, args.AvailableDuty);
        }

        private IEnumerable<DutyType> GetDutyTypes()
        {
            var duties = new List<DutyType>();

            if (_settingsManager.CloseRoadSettings.AutoCleanBodies)
                duties.Add(DutyType.CleanCorpses);
            if (_settingsManager.CloseRoadSettings.AutoCleanWrecks)
                duties.Add(DutyType.CleanWrecks);

            return duties;
        }

        private static Road GetPositionBehindSlot(BlockSlot slot, int index)
        {
            return RoadUtils.GetClosestRoad(slot.Position + MathHelper.ConvertHeadingToDirection(slot.PedHeading) * (80f * index), RoadType.All);
        }

        #endregion
    }
}