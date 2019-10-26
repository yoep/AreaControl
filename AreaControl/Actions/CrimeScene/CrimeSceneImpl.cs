using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Duties;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using AreaControl.Utils;
using Rage;
using RAGENativeUI.Elements;
using VehicleType = AreaControl.Instances.VehicleType;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeSceneImpl : AbstractCrimeScene, ICrimeScene
    {
        private const float InitialAcceptedDistance = 40f;
        private const float AcceptedDistance = 2.5f;
        private const float InitialDrivingSpeed = 30f;
        private const float SlowDrivingSpeed = 8f;

        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly IEntityManager _entityManager;
        private readonly IDutyManager _dutyManager;
        private readonly List<PlaceObjectsDuty.PlaceObject> _placedObjects = new List<PlaceObjectsDuty.PlaceObject>();

        private CrimeSceneSlot _crimeSceneSlot;
        private ACVehicle _police;
        private ACVehicle _ambulance;
        private ACVehicle _fireTruck;

        public CrimeSceneImpl(IRage rage, ILogger logger, IEntityManager entityManager, IDutyManager dutyManager)
        {
            _rage = rage;
            _logger = logger;
            _entityManager = entityManager;
            _dutyManager = dutyManager;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.CreateCrimeScene, AreaControl.CrimeSceneDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.AREA_CONTROL;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        public void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                RemoveCrimeScene();
            }
            else
            {
                CreateCrimeScene();
            }
        }

        #endregion

        #region Functions

        private void RemoveCrimeScene()
        {
            _logger.Info("A crime scene is being removed");
            MenuItem.Text = AreaControl.CreateCrimeScene;
            LspdfrUtils.PlayScannerAudio("WE_ARE_CODE_4");
            _dutyManager.DismissDuties();
            _entityManager.Dismiss();
            ClearBarriers();
            ClearVehicles();
            IsActive = false;
        }

        private void ClearBarriers()
        {
            _rage.NewSafeFiber(() =>
            {
                _placedObjects.ForEach(x => PropUtils.Remove(x.Instance));
                _placedObjects.Clear();
            }, "CrimeSceneImpl.ClearBarriers");
        }

        private void ClearVehicles()
        {
            _rage.NewSafeFiber(() =>
            {
                _logger.Debug("Police wandering around");
                _police.Wander();
            }, "CrimeSceneImpl.ClearVehicles.Police");
            _rage.NewSafeFiber(() =>
            {
                _logger.Debug("Ambulance wandering around");
                _ambulance.Wander();
            }, "CrimeSceneImpl.ClearVehicles.Ambulance");
            _rage.NewSafeFiber(() =>
            {
                _logger.Debug("Firetruck wandering around");
                _fireTruck.Wander();
            }, "CrimeSceneImpl.ClearVehicles.FireTruck");
        }

        private void CreateCrimeScene()
        {
            _logger.Info("A crime scene is being created");
            IsActive = true;
            MenuItem.Text = AreaControl.RemoveCrimeScene;
            _rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                _crimeSceneSlot = DetermineCrimeSceneSlot();

                _rage.DisplayNotification($"Requesting dispatch for a ~b~crime scene~s~ at {World.GetStreetName(position)}");
                LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", position, true);
                LspdfrUtils.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");

                ExecuteAmbulanceActions();
                ExecuteFireTruckActions();
                ExecutePoliceActions();
            }, "CrimeSceneImpl.CreateCrimeScene");
        }

        private void ExecuteAmbulanceActions()
        {
            _rage.NewSafeFiber(() =>
            {
                var ambulancePosition = _crimeSceneSlot.Ambulance.Position;
                var crimeSceneHeading = _crimeSceneSlot.StartLane.Heading;
                var spawnPosition = GetSpawnPositionAmbulance(ambulancePosition, crimeSceneHeading);

                _ambulance = _entityManager.CreateVehicleAt(spawnPosition, crimeSceneHeading, VehicleType.Ambulance, 2);
                _ambulance.EnableEmergencyLights();
                DriveTo(_ambulance, _crimeSceneSlot.Ambulance);
                _ambulance.Occupants.ForEach(x => x.LeaveVehicle(LeaveVehicleFlags.None));

                AssignNextAvailableDutyToMedicPassenger();
            }, "CrimeSceneImpl.ExecuteAmbulanceActions");
        }

        private void AssignNextAvailableDutyToMedicPassenger()
        {
            var ped = _ambulance.Passengers.First();
            var duty = _dutyManager.NextAvailableDuty(ped, DutyTypeFlag.MedicDuties);

            if (duty != null)
            {
                duty.OnCompletion += (sender, args) => AssignNextAvailableDutyToMedicPassenger();
                return;
            }

            var listener = _dutyManager[ped];

            listener.DutyTypes = DutyTypeFlag.MedicDuties;
            listener.OnDutyAvailable += (sender, args) => AssignNextAvailableDutyToMedicPassenger();
        }

        private void ExecuteFireTruckActions()
        {
            _rage.NewSafeFiber(() =>
            {
                var firetruckPosition = _crimeSceneSlot.Firetruck.Position;
                var crimeSceneHeading = _crimeSceneSlot.StartLane.Heading;
                var spawnPosition = GetSpawnPositionFireTruck(firetruckPosition, crimeSceneHeading);

                _fireTruck = _entityManager.CreateVehicleAt(spawnPosition, crimeSceneHeading, VehicleType.FireTruck, 2);
                _fireTruck.EnableEmergencyLights();
                DriveTo(_fireTruck, _crimeSceneSlot.Firetruck);
                _fireTruck.Occupants.ForEach(x => x.LeaveVehicle(LeaveVehicleFlags.None));
            }, "CrimeSceneImpl.ExecuteFireTruckActions");
        }

        private void ExecutePoliceActions()
        {
            _rage.NewSafeFiber(() =>
            {
                var policeSlot = _crimeSceneSlot.Police;
                var policePosition = policeSlot.Position;
                var crimeSceneHeading = _crimeSceneSlot.StartLane.Heading;
                var spawnPosition = GetSpawnPositionPolice(policePosition, crimeSceneHeading);

                _police = _entityManager.CreateVehicleAt(spawnPosition, crimeSceneHeading, VehicleType.Police, 2);
                _police.EnableEmergencyLights();
                DriveTo(_police, _crimeSceneSlot.Police);

                ExecutePoliceDriverActions();
                ExecutePolicePassengerActions();
            }, "CrimeSceneImpl.ExecutePoliceActions");
        }

        private void ExecutePoliceDriverActions()
        {
            var ped = _police.Driver;
            var policeSlot = _crimeSceneSlot.Police;
            var barriers = policeSlot.Barriers.Select(x => x.Object).ToList();
            _placedObjects.AddRange(barriers);

            ped.LeaveVehicle(LeaveVehicleFlags.None)
                .WaitForCompletion();

            _dutyManager
                .NewPlaceObjectsDuty(ped, barriers, ResponseCode.Code3, false)
                .OnCompletion = (sender, args) =>
                _dutyManager.NewRedirectTrafficDuty(ped, policeSlot.PedPosition, policeSlot.PedHeading, ResponseCode.Code3);
        }

        private void ExecutePolicePassengerActions()
        {
            var ped = _police.Passengers.First();
            var barriers = _crimeSceneSlot.BarriersAndCones.Select(x => x.Object).ToList();
            _placedObjects.AddRange(barriers);

            ped.LeaveVehicle(LeaveVehicleFlags.None)
                .WaitForCompletion();
            
            _logger.Trace("Placing down crime scene barriers...");
            _dutyManager
                .NewPlaceObjectsDuty(ped, barriers, ResponseCode.Code3, false)
                .OnCompletion += (sender, args) => _dutyManager.NextAvailableDuty(ped, DutyTypeFlag.ReturnToVehicle);
        }

        private static void DriveTo(ACVehicle vehicle, IVehicleSlot slot)
        {
            var slotPosition = slot.Position;
            var driveToPosition = vehicle.DriveToPosition(slotPosition, InitialDrivingSpeed, VehicleDrivingFlags.Emergency, InitialAcceptedDistance);

            driveToPosition.OnAborted += (sender, args) => VehicleUtils.WarpVehicle(vehicle, slot);
            driveToPosition.WaitForCompletion(30000);

            var getIntoPosition = vehicle.DriveToPosition(slotPosition, SlowDrivingSpeed, VehicleDrivingFlags.Emergency, AcceptedDistance);
            getIntoPosition.OnCompletionOrAborted += (sender, args) => VehicleUtils.WarpVehicle(vehicle, slot);
            getIntoPosition.WaitForCompletion(1000);
        }

        #endregion
    }
}