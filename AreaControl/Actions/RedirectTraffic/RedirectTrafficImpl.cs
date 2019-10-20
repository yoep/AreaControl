using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
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
using VehicleType = AreaControl.Instances.VehicleType;

namespace AreaControl.Actions.RedirectTraffic
{
    public class RedirectTrafficImpl : AbstractRedirectTraffic
    {
        private const float ScanRadius = 100f;
        private const float VehiclePositionTolerance = 1f;
        private const float VehicleHeadingTolerance = 20f;
        private const string DispatchAudio = "WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION";

        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

        private readonly List<PlaceObjectsDuty.PlaceObject> _placedObjects = new List<PlaceObjectsDuty.PlaceObject>();
        private RedirectSlot _redirectSlot;

        #region Constructor

        public RedirectTrafficImpl(IRage rage, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager,
            ISettingsManager settingsManager, ILogger logger)
        {
            _rage = rage;
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
            _settingsManager = settingsManager;
            _logger = logger;
        }

        #endregion

        #region IMenuComponent implementation

        /// <inheritdoc />
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RedirectTraffic, AreaControl.RedirectTrafficDescription);

        /// <inheritdoc />
        public override MenuType Type => MenuType.STREET_CONTROL;

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
                _placedObjects.ForEach(x => PropUtils.Remove(x.Instance));
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
                LspdfrUtils.PlayScannerAudioUsingPosition(DispatchAudio + " " + _responseManager.ResponseCodeAudio, position, true);
                LspdfrUtils.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");

                var spawnPosition = GetSpawnPosition(_redirectSlot);
                var vehicle = _entityManager.FindVehicleWithinOrCreateAt(_redirectSlot.Position, spawnPosition, VehicleType.Police, ScanRadius, 1);

                vehicle.CreateBlip();
                
                MoveToSlot(_redirectSlot, vehicle);

                LeaveVehicle(vehicle);

                PlaceSceneryItems(vehicle.Driver, _redirectSlot);
                AssignRedirectTrafficDutyToDriver(vehicle, _redirectSlot);
            }, "RedirectTrafficImpl.RedirectTraffic");
        }

        private void MoveToSlot(RedirectSlot redirectSlot, ACVehicle vehicle)
        {
            var vehicleDriver = vehicle.Driver;
            var initialDrivingFlags = _responseManager.VehicleDrivingFlags;
            var initialDrivingSpeed = _responseManager.VehicleSpeed;

            vehicle.EnableEmergencyLights();
            
            if (_responseManager.ResponseCode == ResponseCode.Code3)
                vehicle.EnableSirens();

            _logger.Debug("Vehicle driving to redirect traffic slot...");
            vehicleDriver.Instance.Tasks
                .DriveToPosition(redirectSlot.Position, initialDrivingSpeed, initialDrivingFlags, 35f)
                .WaitForCompletion();
            _logger.Debug("Vehicle arrived in the area of redirect traffic slot " + redirectSlot);
            vehicle.EnableEmergencyLights();
            redirectSlot.ClearSlotFromTraffic();
            vehicleDriver.Instance.Tasks
                .DriveToPosition(redirectSlot.Position, 10f, VehicleDrivingFlags.Emergency, 1f)
                .WaitForCompletion(15000);
            VehicleUtils.WarpVehicle(vehicle, redirectSlot, VehicleHeadingTolerance, VehiclePositionTolerance);
            vehicle.EnableHazardIndicators();
        }

        private void LeaveVehicle(ACVehicle vehicle)
        {
            var ped = vehicle.Driver;
            var leaveVehicleTask = ped.LeaveVehicle(LeaveVehicleFlags.None);

            leaveVehicleTask.OnAborted += (sender, args) => ped.WarpOutVehicle();
            leaveVehicleTask.WaitForCompletion(5000);
            _logger.Trace("Leave vehicle task executor ended with: " + leaveVehicleTask);
            
            ped.CreateBlip();
            vehicle.EnableSirens();
        }

        private void AssignRedirectTrafficDutyToDriver(ACVehicle vehicle, RedirectSlot redirectSlot)
        {
            var driver = vehicle.Driver;
            
            _dutyManager.NewRedirectTrafficDuty(driver, redirectSlot.PedPosition, redirectSlot.PedHeading, _responseManager.ResponseCode);
        }

        private void PlaceSceneryItems(ACPed ped, RedirectSlot redirectSlot)
        {
            var redirectTrafficSettings = GetRedirectTrafficSettings();

            if (!redirectTrafficSettings.PlaceSceneryItems)
                return;

            //walk to front of car to prevent being stuck at the side when placing cones on the right side of the vehicle
            if (redirectSlot.PlaceConesRightSide)
            {
                ped.WalkTo(redirectSlot.Position + MathHelper.ConvertHeadingToDirection(redirectSlot.Heading) * 3f, redirectSlot.Heading)
                    .WaitForCompletion(5000);
            }

            // add cones to place object duty
            foreach (var cone in redirectSlot.Cones)
            {
                _placedObjects.Add(cone.Object);
            }

            // add sign to place object duty
            _placedObjects.Add(redirectSlot.Sign.Object);

            // add sign light to place object duty of required
            if (redirectTrafficSettings.AlwaysPlaceLight || GameUtils.TimePeriod == TimePeriod.Evening || GameUtils.TimePeriod == TimePeriod.Night)
            {
                _logger.Trace("Current game time " + GameUtils.GetCurrentHour() + " (" + GameUtils.TimePeriod + ")");
                _logger.Debug("Placing down redirect traffic light...");
                _placedObjects.Add(redirectSlot.SignLight.Object);
            }

            _dutyManager.NewPlaceObjectsDuty(ped, _placedObjects, _responseManager.ResponseCode, true);
        }

        private RedirectTrafficSettings GetRedirectTrafficSettings()
        {
            return _settingsManager.RedirectTrafficSettings;
        }

        private static Vector3 GetSpawnPosition(RedirectSlot redirectSlot)
        {
            var positionBehindSlot = redirectSlot.Position + MathHelper.ConvertHeadingToDirection(redirectSlot.PedHeading) * 80f;
            var closestRoad = RoadUtils.GetClosestRoad(positionBehindSlot, RoadType.All);

            return closestRoad.Position;
        }

        #endregion
    }
}