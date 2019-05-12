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
using RAGENativeUI.Elements;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectTrafficImpl : AbstractRedirectTraffic
    {
        private const float ScanRadius = 250f;
        private const float VehiclePositionTolerance = 0.5f;
        private const float VehicleHeadingTolerance = 20f;
        private const string DispatchAudio = "WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION";

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

        private readonly List<PlaceObjectsDuty.PlaceObject> _placedObjects = new List<PlaceObjectsDuty.PlaceObject>();
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
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RedirectTraffic, AreaControl.RedirectTrafficDescription);

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
                _placedObjects.ForEach(x => PropUtil.Remove(x.Instance));
                IsActive = false;
            }, "RedirectTrafficImpl.RemoveRedirectTraffic");
        }

        private void RedirectTraffic()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.RedirectTrafficRemove;
            _rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                _redirectSlot = _redirectSlot ?? DetermineRedirectSlot();

                _rage.DisplayNotification("Requesting dispatch to ~b~redirect traffic~s~...");
                Functions.PlayScannerAudioUsingPosition(DispatchAudio + " " + _responseManager.ResponseCodeAudio, position);
                GameFiber.Sleep(5000);
                Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");

                var spawnPosition = GetSpawnPosition(_redirectSlot);
                var vehicle = _entityManager.FindVehicleWithinOrCreateAt(_redirectSlot.Position, spawnPosition, ScanRadius, 1);

                MoveToSlot(_redirectSlot, vehicle);

                vehicle.Driver.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                vehicle.EnableSirens();

                PlaceCones(vehicle.Driver, _redirectSlot);
                AssignRedirectTrafficDutyToDriver(vehicle, _redirectSlot);
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
            redirectSlot.ClearSlotFromTraffic();
            vehicleDriver.Instance.Tasks
                .DriveToPosition(redirectSlot.Position, 10f, VehicleDrivingFlags.Emergency, 1f)
                .WaitForCompletion(30000);
            WarpInPositionIfNeeded(vehicle, redirectSlot);
            vehicle.EnableHazardIndicators();
        }

        private void AssignRedirectTrafficDutyToDriver(ACVehicle vehicle, RedirectSlot redirectSlot)
        {
            var driver = vehicle.Driver;
            _dutyManager.RegisterDuty(driver, new RedirectTrafficDuty(redirectSlot.PedPosition, redirectSlot.PedHeading, _responseManager.ResponseCode));
        }

        private void PlaceCones(ACPed ped, RedirectSlot redirectSlot)
        {
            if (!_settingsManager.RedirectTrafficSettings.PlaceCones)
                return;

            //walk to front of car to prevent being stuck at the side when placing cones on the right side of the vehicle
            if (redirectSlot.PlaceConesRightSide)
            {
                ped.WalkTo(redirectSlot.Position + MathHelper.ConvertHeadingToDirection(redirectSlot.Heading) * 3f, redirectSlot.Heading)
                    .WaitForCompletion(5000);
            }

            foreach (var cone in redirectSlot.Cones)
            {
                _placedObjects.Add(new PlaceObjectsDuty.PlaceObject(cone.Position, 0f,
                    (pos, heading) => PropUtil.CreateSmallConeWithStripes(pos)));
            }

            _dutyManager.RegisterDuty(ped,
                new PlaceObjectsDuty(_dutyManager.GetNextDutyId(), _placedObjects, _responseManager.ResponseCode, true));
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

        #endregion
    }
}