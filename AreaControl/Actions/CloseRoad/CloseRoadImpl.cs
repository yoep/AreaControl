using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Duties;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using AreaControl.Settings;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;
using VehicleType = AreaControl.Instances.VehicleType;

namespace AreaControl.Actions.CloseRoad
{
    [IoC.Primary]
    public class CloseRoadImpl : AbstractCloseRoad
    {
        private const float ScanRadius = 250f;

        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

        private ICollection<PoliceSlot> _blockSlots;

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
                LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION " + _responseManager.ResponseCodeAudio,
                    position, true);
                _blockSlots = DetermineBlockSlots();

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

        private void SpawnBlockSlots(ICollection<PoliceSlot> blockSlots)
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
                        var vehicle = _entityManager.FindVehicleWithinOrCreateAt(slot.Position, positionBehindSlot.Position, VehicleType.Police, ScanRadius, 2);
                        vehicle.SetOccupantsBusyState(true);
                        Logger.Debug($"Using vehicle {vehicle} for block slot {index}");

                        MoveToSlot(vehicle, slot);
                        AssignDutiesToDriver(vehicle.Driver, slot);
                        AssignAvailableDutiesToPassengers(vehicle);
                    }, "BlockSlot#" + i);
            }
        }

        private void MoveToSlot(ACVehicle vehicle, PoliceSlot slot)
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
            VehicleUtils.WarpVehicle(vehicle, slot);
            Logger.Trace("Vehicle parked at block slot " + slot);
        }

        private void AssignDutiesToDriver(ACPed ped, PoliceSlot slot)
        {
            ped.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
            AssignPlaceBarriersDuty(ped, slot);

            _dutyManager.NewRedirectTrafficDuty(ped, slot.PedPosition, slot.PedHeading, _responseManager.ResponseCode);
        }

        private void AssignAvailableDutiesToPassengers(ACVehicle vehicle)
        {
            var passengers = vehicle.Passengers;
            passengers.ForEach(x =>
            {
                x.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                AssignNextAvailableDutyToPed(x);
            });
        }

        private void AssignPlaceBarriersDuty(ACPed ped, PoliceSlot slot)
        {
            if (!_settingsManager.CloseRoadSettings.PlaceBarriers)
                return;

            var objects = slot.Barriers
                .Select(barrier => barrier.Object)
                .ToList();

            var placeObjectsDuty = _dutyManager.NewPlaceObjectsDuty(ped, objects, _responseManager.ResponseCode, false);
            Logger.Debug("Created place barriers duty " + placeObjectsDuty);
        }

        private void AssignNextAvailableDutyToPed(ACPed ped)
        {
            var nextAvailableDuty = _dutyManager.NextAvailableDuty(ped, DutyTypeFlag.CleanDuties);

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

            dutyListener.DutyTypes = DutyTypeFlag.CleanDuties;
            dutyListener.OnDutyAvailable += (sender, args) => ActivateNonIdleDuty(ped, args.AvailableDuty);
        }

        private static Road GetPositionBehindSlot(PoliceSlot slot, int index)
        {
            return RoadUtils.GetClosestRoad(slot.Position + MathHelper.ConvertHeadingToDirection(slot.PedHeading) * (80f * index), RoadType.All);
        }

        #endregion
    }
}