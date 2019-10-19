using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Duties;
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
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly IEntityManager _entityManager;
        private readonly IDutyManager _dutyManager;

        private CrimeSceneSlot _crimeSceneSlot;
        private ACVehicle _police;
        private ACVehicle _ambulance;
        private ACVehicle _fireTruck;
        private IEnumerable<PlaceObjectsDuty.PlaceObject> _placedObjects;

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
            _placedObjects?.ToList().ForEach(x => PropUtils.Remove(x.Instance));
        }

        private void ClearVehicles()
        {
            _rage.NewSafeFiber(() => _police.Wander(), "ClearVehicles.Police");
            _rage.NewSafeFiber(() => _ambulance.Wander(), "ClearVehicles.Ambulance");
            _rage.NewSafeFiber(() => _fireTruck.Wander(), "ClearVehicles.FireTruck");
        }

        private void CreateCrimeScene()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.RemoveCrimeScene;
            _rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                _crimeSceneSlot = DetermineCrimeSceneSlot();

                _rage.DisplayNotification("Requesting dispatch to ~b~create crime scene~s~ " + World.GetStreetName(position) + "...");
                LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", position, true);

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
                _ambulance.DriveToPosition(ambulancePosition, 45f, VehicleDrivingFlags.Emergency, 35f)
                    .WaitForCompletion();
                _ambulance.DriveToPosition(ambulancePosition, 10f, VehicleDrivingFlags.Emergency, 1f)
                    .WaitForCompletion();
                VehicleUtils.WarpVehicle(_ambulance, _crimeSceneSlot.Ambulance);
                _ambulance.Occupants.ForEach(x => x.LeaveVehicle(LeaveVehicleFlags.None));
            }, "CrimeSceneImpl.ExecuteAmbulanceActions");
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
                _fireTruck.DriveToPosition(firetruckPosition, 45f, VehicleDrivingFlags.Emergency, 35f)
                    .WaitForCompletion();
                _fireTruck.DriveToPosition(firetruckPosition, 10f, VehicleDrivingFlags.Emergency, 1f)
                    .WaitForCompletion();
                VehicleUtils.WarpVehicle(_fireTruck, _crimeSceneSlot.Firetruck);
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
                _police.DriveToPosition(policePosition, 45f, VehicleDrivingFlags.Emergency, 35f)
                    .WaitForCompletion();
                _police.DriveToPosition(policePosition, 10f, VehicleDrivingFlags.Emergency, 2f)
                    .WaitForCompletion();
                VehicleUtils.WarpVehicle(_police, policeSlot);
                _police.Occupants.ForEach(x => x.LeaveVehicle(LeaveVehicleFlags.None));

                _dutyManager.NewRedirectTrafficDuty(_police.Driver, policeSlot.PedPosition, policeSlot.PedHeading, ResponseCode.Code3);
                ExecutePolicePassengerActions();
            }, "CrimeSceneImpl.ExecutePoliceActions");
        }

        private void ExecutePolicePassengerActions()
        {
            var passenger = _police.Passengers.First();
            _placedObjects = _crimeSceneSlot.Barriers.Select(x => x.Object);
            
            _dutyManager
                .NewPlaceObjectsDuty(passenger, _placedObjects, ResponseCode.Code3, false)
                .OnCompletion += (sender, args) => _dutyManager.NewIdleDuty(passenger);
        }

        #endregion
    }
}