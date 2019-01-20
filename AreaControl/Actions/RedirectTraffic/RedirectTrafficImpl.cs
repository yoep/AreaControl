using System;
using System.Collections.Generic;
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
using Object = Rage.Object;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectTrafficImpl : AbstractRedirectTraffic, IPreviewSupport
    {
        private const float ScanRadius = 250f;
        private const float VehiclePositionTolerance = 2f;
        private const float VehicleHeadingTolerance = 20f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

        private readonly List<Object> _cones = new List<Object>();
        private RedirectSlot _redirectSlot;

        #region Constructor

        public RedirectTrafficImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager,
            ISettingsManager settingsManager)
        {
            _rage = rage;
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
            _settingsManager = settingsManager;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuListItem(AreaControl.RedirectTraffic, AreaControl.RedirectTrafficDescription,
            new List<IDisplayItem>
            {
                new DisplayItem(-20f, "1"),
                new DisplayItem(-15f, "2"),
                new DisplayItem(-10f, "3"),
                new DisplayItem(-5f, "4"),
                new DisplayItem(0f, "5"),
                new DisplayItem(5f, "6"),
                new DisplayItem(10f, "7"),
                new DisplayItem(15f, "8"),
                new DisplayItem(20f, "9"),
                new DisplayItem(25f, "10")
            });

        /// <inheritdoc />
        public override bool IsVisible => true;

        /// <inheritdoc />
        public override void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                RemoveRedirectTraffic();
            }
            else
            {
                RedirectTraffic();
            }
        }

        public override void OnMenuHighlighted(IMenu sender)
        {
            if (_settingsManager.RedirectTrafficSettings.ShowPreview && !IsActive)
                _rage.NewSafeFiber(() =>
                {
                    ((UIMenuListItem) MenuItem).Index = 4;
                    
                    while (sender.IsShown && MenuItem.Selected && !IsActive)
                    {
                        var distanceFromOriginalSlot = GetDistanceFromOriginalSlot();
                        var redirectSlot = DetermineRedirectSlot();

                        if (redirectSlot.Position != _redirectSlot?.Position || !IsPreviewActive)
                        {
                            DeletePreview();
                            _redirectSlot = redirectSlot;
                            CreatePreview();
                        }

                        GameFiber.Sleep(500);
                    }

                    DeletePreview();
                }, "RedirectTrafficImpl.OnMenuHighlighted");
        }

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                IsPreviewActive = true;
                _redirectSlot?.CreatePreview();
            }, "RedirectTrafficImpl.CreatePreview");
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _rage.NewSafeFiber(() =>
            {
                IsPreviewActive = false;
                _redirectSlot?.DeletePreview();
            }, "RedirectTrafficImpl.DeletePreview");
        }

        #endregion

        #region Functions

        private void RemoveRedirectTraffic()
        {
            MenuItem.Text = AreaControl.RedirectTraffic;
            _rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_ARE_CODE_4");
                _dutyManager.DismissDuties();
                _entityManager.Dismiss();
                _cones.ForEach(PropUtil.Remove);
                IsActive = false;
            }, "RedirectTrafficImpl.RemoveRedirectTraffic");
        }

        private void RedirectTraffic()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.RedirectTrafficRemove;
            _rage.NewSafeFiber(() =>
            {
                DeletePreview();
                Functions.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION " + _responseManager.ResponseCodeAudio,
                    Game.LocalPlayer.Character.Position);
                var distanceFromOriginalSlot = GetDistanceFromOriginalSlot();
                var redirectSlot = _redirectSlot ?? DetermineRedirectSlot();

                Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");
                var spawnPosition = GetSpawnPosition(redirectSlot);
                var vehicle = _entityManager.FindVehicleWithinOrCreateAt(redirectSlot.Position, spawnPosition, ScanRadius, 1);

                MoveToSlot(redirectSlot, vehicle);
                PlaceCones(vehicle.Driver, redirectSlot);
                AssignRedirectTrafficDutyToDriver(vehicle, redirectSlot);
            }, "RedirectTrafficImpl.RedirectTraffic");
        }

        private void MoveToSlot(RedirectSlot redirectSlot, ACVehicle vehicle)
        {
            var vehicleDriver = vehicle.Driver;
            var initialDrivingFlags = _responseManager.VehicleDrivingFlags;
            var initialDrivingSpeed = _responseManager.VehicleSpeed;

            if (_responseManager.ResponseCode == ResponseCode.Code3)
                vehicle.EnableSirens();

            _rage.LogTrivialDebug("Vehicle driving to redirect traffic slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(redirectSlot.Position, initialDrivingSpeed, initialDrivingFlags, 35f)
                .WaitForCompletion();
            _rage.LogTrivialDebug("Vehicle arrived in the area of redirect traffic slot " + redirectSlot);
            vehicle.EnableEmergencyLights();
            vehicleDriver.Instance.Tasks
                .DriveToPosition(redirectSlot.Position, 10f, VehicleDrivingFlags.Emergency, 1f)
                .WaitForCompletion(30000);
            WarpInPositionIfNeeded(vehicle, redirectSlot);
            vehicle.EnableHazardIndicators();

            var emptyVehicleTask = vehicle.Empty()
                .WaitForCompletion(10000);
            _rage.LogTrivialDebug("Empty vehicle task ended with " + emptyVehicleTask);
        }

        private void AssignRedirectTrafficDutyToDriver(ACVehicle vehicle, RedirectSlot redirectSlot)
        {
            var driver = vehicle.Driver;
            var trafficDuty = new RedirectTrafficDuty(redirectSlot.PedPosition, redirectSlot.PedHeading, _responseManager.ResponseCode);
            _dutyManager.RegisterDuty(driver, trafficDuty);
            trafficDuty.Execute();
        }

        private void PlaceCones(ACPed ped, RedirectSlot redirectSlot)
        {
            if (!_settingsManager.RedirectTrafficSettings.PlaceCones)
                return;

            foreach (var cone in redirectSlot.Cones)
            {
                var positionBehindCone = cone.Position + MathHelper.ConvertHeadingToDirection(cone.Heading) * 0.5f;
                var walkToExecutor = _responseManager.ResponseCode == ResponseCode.Code2
                    ? ped.WalkTo(positionBehindCone, RoadUtil.OppositeHeading(cone.Heading))
                    : ped.RunTo(positionBehindCone, RoadUtil.OppositeHeading(cone.Heading));
                var animationExecutor = walkToExecutor
                    .WaitForAndExecute(executor =>
                    {
                        _rage.LogTrivialDebug("Completed walk to cone for " + executor);
                        var coneProp = PropUtil.CreateSmallConeWithStripes(cone.Position);
                        _cones.Add(coneProp);
                        return AnimationUtil.PlaceDownObject(ped, coneProp);
                    }, 20000)
                    .WaitForCompletion(2000);
                _rage.LogTrivialDebug("Completed place cone animation for " + animationExecutor);
            }
        }

        private static Vector3 GetSpawnPosition(RedirectSlot redirectSlot)
        {
            var positionBehindSlot = redirectSlot.Position + MathHelper.ConvertHeadingToDirection(redirectSlot.PedHeading) * 80f;
            var closestRoad = RoadUtil.GetClosestRoad(positionBehindSlot, RoadType.All);

            return closestRoad.Position;
        }

        private static void WarpInPositionIfNeeded(ACVehicle vehicle, RedirectSlot redirectSlot)
        {
            var positionDifference = Vector3.Distance2D(vehicle.Instance.Position, redirectSlot.Position);
            var headingDifference = Math.Abs(vehicle.Instance.Heading - redirectSlot.Heading);

            if (positionDifference > VehiclePositionTolerance)
                vehicle.Instance.Position = redirectSlot.Position;
            if (headingDifference > VehicleHeadingTolerance)
                vehicle.Instance.Heading = redirectSlot.Heading;
        }

        private float GetDistanceFromOriginalSlot()
        {
            return (float) ((UIMenuListItem) MenuItem).SelectedValue;
        }

        #endregion
    }
}