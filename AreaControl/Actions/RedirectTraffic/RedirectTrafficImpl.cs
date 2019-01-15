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
    public class RedirectTrafficImpl : AbstractRedirectTraffic, IPreviewSupport
    {
        private const float ScanRadius = 250f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

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
        public override UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.RedirectTraffic);

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
                    while (sender.IsShown && MenuItem.Selected)
                    {
                        var redirectSlot = DetermineRedirectSlot();

                        if (redirectSlot.Position != _redirectSlot?.Position)
                        {
                            DeletePreview();
                            _redirectSlot = redirectSlot;
                            CreatePreview();
                        }
                        
                        GameFiber.Sleep(1000);
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
            if (!IsPreviewActive)
                return;

            _rage.NewSafeFiber(() =>
            {
                IsPreviewActive = false;
                _redirectSlot?.DeletePreview();
            }, "RedirectTrafficImpl.DeletePreview");
        }

        #endregion

        private void RemoveRedirectTraffic()
        {
            MenuItem.Text = AreaControl.RedirectTraffic;
            _rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_ARE_CODE_4");
                _dutyManager.DismissDuties();
                _entityManager.Dismiss();
                IsActive = false;
            }, "RedirectTrafficImpl.RemoveRedirectTraffic");
        }

        private void RedirectTraffic()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.RedirectTrafficRemove;
            _rage.NewSafeFiber(() =>
            {
                Functions.PlayScannerAudio("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE " + _responseManager.ResponseCodeAudio);
                var redirectSlot = _redirectSlot ?? DetermineRedirectSlot();
                DeletePreview();

                var spawnPosition = GetSpawnPosition(redirectSlot);
                var vehicle = _entityManager.FindVehicleWithinOrCreateAt(redirectSlot.Position, spawnPosition, ScanRadius, 1);

                MoveToSlot(redirectSlot, vehicle);
                //TODO: place cones
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
            vehicle.EnableHazardIndicators();

            var emptyVehicleTask = vehicle.Empty()
                .WaitForCompletion(10000);
            _rage.LogTrivialDebug("Empty vehicle task ended with " + emptyVehicleTask);
        }

        private void AssignRedirectTrafficDutyToDriver(ACVehicle vehicle, RedirectSlot redirectSlot)
        {
            var trafficDuty = new RedirectTrafficDuty(redirectSlot.PedPosition, redirectSlot.PedHeading);
            _dutyManager.RegisterDuty(trafficDuty);
            vehicle.Driver.ActivateDuty(trafficDuty);
        }

        private static Vector3 GetSpawnPosition(RedirectSlot redirectSlot)
        {
            var positionBehindSlot = redirectSlot.Position + MathHelper.ConvertHeadingToDirection(redirectSlot.PedHeading) * 80f;
            var closestRoad = RoadUtil.GetClosestRoad(positionBehindSlot, RoadType.All);

            return closestRoad.Position;
        }
    }
}