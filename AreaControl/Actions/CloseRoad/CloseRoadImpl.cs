using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Settings;
using AreaControl.Utils;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CloseRoad
{
    [IoC.Primary]
    public class CloseRoadImpl : AbstractCloseRoad, IPreviewSupport
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

        public CloseRoadImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager,
            ISettingsManager settingsManager)
            : base(rage)
        {
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
            _settingsManager = settingsManager;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuListItem(AreaControl.ActionCloseRoad, AreaControl.ActionCloseRoadDescription,
            new List<IDisplayItem>
            {
                new DisplayItem(-20f, "-4"),
                new DisplayItem(-15f, "-3"),
                new DisplayItem(-10f, "-2"),
                new DisplayItem(-5f, "-1"),
                new DisplayItem(0f, "0"),
                new DisplayItem(5f, "+1"),
                new DisplayItem(10f, "+2"),
                new DisplayItem(15f, "+3"),
                new DisplayItem(20f, "+4"),
                new DisplayItem(25f, "+5")
            });

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

        /// <inheritdoc />
        public override void OnMenuHighlighted(IMenu sender)
        {
            if (_settingsManager.RedirectTrafficSettings.ShowPreview && !IsActive)
                Rage.NewSafeFiber(() =>
                {
                    while (sender.IsShown && MenuItem.Selected && !IsActive)
                    {
                        var blockSlots = DetermineBlockSlots(GetDistanceFromOriginalSlot());
                        var blockSlot = blockSlots.First();
                        var bufferedBlockSlot = _blockSlots?.First();

                        if (blockSlot.Position != bufferedBlockSlot?.Position || !IsPreviewActive)
                        {
                            if (_blockSlots != null)
                            {
                                foreach (var slot in _blockSlots)
                                {
                                    slot.DeletePreview();
                                }
                            }
                            
                            _blockSlots = blockSlots;
                            CreatePreview();
                        }

                        GameFiber.Sleep(250);
                    }

                    DeletePreview();
                }, "CloseRoadImpl.OnMenuHighlighted");
        }

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            Rage.NewSafeFiber(() =>
            {
                IsPreviewActive = true;
                foreach (var slot in _blockSlots)
                {
                    slot.CreatePreview();
                }
            }, "CloseRoadImpl.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            IsPreviewActive = false;

            if (_blockSlots == null || _blockSlots.Count == 0)
                return;

            Rage.NewSafeFiber(() =>
            {
                foreach (var slot in _blockSlots)
                {
                    slot.DeletePreview();
                }
            }, "CloseRoadImpl.DeletePreview");
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            ((UIMenuListItem) MenuItem).Index = 4;
        }

        private void OpenRoad()
        {
            MenuItem.Text = AreaControl.ActionCloseRoad;
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            _dutyManager.DismissDuties();
            _entityManager.Dismiss();
            IsActive = false;
        }

        private void CloseRoad()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.ActionOpenRoad;
            Rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION " + _responseManager.ResponseCodeAudio,
                    Game.LocalPlayer.Character.Position);
                var blockSlots = DetermineBlockSlots(GetDistanceFromOriginalSlot());

                GameFiber.Sleep(2000);
                if (blockSlots.Count > 0)
                {
                    SpawnBlockSlots(blockSlots);
                }
                else
                {
                    Rage.LogTrivial("Unable to create any road block slots");
                }
            }, "AreaControl.CloseRoad");
        }

        private void SpawnBlockSlots(ICollection<BlockSlot> blockSlots)
        {
            var i = 0;

            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");
            Rage.LogTrivialDebug("Spawning " + blockSlots.Count + " block slot(s) for close road function");
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
                        Rage.LogTrivialDebug("Using vehicle " + vehicle + " for block slot " + index);

                        MoveToSlot(vehicle, slot);
                        AssignRedirectTrafficDutyToDriver(vehicle, slot);
                        AssignAvailableDutiesToPassengers(vehicle);
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
            var driver = vehicle.Driver;
            var trafficDuty = new RedirectTrafficDuty(slot.PedPosition, slot.PedHeading, _responseManager.ResponseCode)
            {
                Ped = driver
            };
            _dutyManager.RegisterDuty(driver, trafficDuty);
            trafficDuty.Execute();
        }

        private void AssignAvailableDutiesToPassengers(ACVehicle vehicle)
        {
            vehicle.Passengers.ForEach(AssignNextAvailableDutyToPed);
        }

        private void AssignNextAvailableDutyToPed(ACPed ped)
        {
            var nextAvailableDuty = _dutyManager.NextAvailableOrIdleDuty(ped);

            if (nextAvailableDuty.GetType() != typeof(ReturnToVehicleDuty))
            {
                ActivateNonIdleDuty(ped, nextAvailableDuty);
            }
            else
            {
                nextAvailableDuty.OnCompletion += (sender, args) => RegisterDutyListenerForIdlePed(ped);
                nextAvailableDuty.Execute();
            }
        }

        private void ActivateNonIdleDuty(ACPed ped, IDuty duty)
        {
            duty.OnCompletion += (sender, args) => AssignNextAvailableDutyToPed(ped);
            duty.Execute();
        }

        private void RegisterDutyListenerForIdlePed(ACPed ped)
        {
            _dutyManager[ped].OnDutyAvailable += (sender, args) => ActivateNonIdleDuty(ped, args.AvailableDuty);
        }
        
        private float GetDistanceFromOriginalSlot()
        {
            return (float) ((UIMenuListItem) MenuItem).SelectedValue;
        }

        private static Road GetPositionBehindSlot(BlockSlot slot, int index)
        {
            return RoadUtil.GetClosestRoad(slot.Position + MathHelper.ConvertHeadingToDirection(slot.PedHeading) * (80f * index), RoadType.All);
        }

        #endregion
    }
}