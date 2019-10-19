using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Settings;
using AreaControl.Utils;
using AreaControl.Utils.Road;
using Rage;
using RAGENativeUI.Elements;
using VehicleType = AreaControl.Instances.VehicleType;

namespace AreaControl.Actions.CleanArea
{
    public class CleanAreaImpl : ICleanArea
    {
        private const float SearchPedRadius = 100f;
        private const float SpawnDistance = 150f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IDutyManager _dutyManager;
        private readonly ISettingsManager _settingsManager;

        public CleanAreaImpl(IRage rage, IEntityManager entityManager, IDutyManager dutyManager, ISettingsManager settingsManager)
        {
            _rage = rage;
            _entityManager = entityManager;
            _dutyManager = dutyManager;
            _settingsManager = settingsManager;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ClearArea, AreaControl.ClearAreaDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.AREA_CONTROL;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            Execute();
        }

        #endregion

        #region Functions

        private void Execute()
        {
            _rage.NewSafeFiber(() =>
            {
                var playerPosition = Game.LocalPlayer.Character.Position;
                var allCops = _entityManager.FindPedsWithin(playerPosition, SearchPedRadius, PedType.Cop);
                var availableCops = allCops
                    .Where(x => !x.IsBusy)
                    .ToList();
                ACVehicle vehicle = null;
                _rage.LogTrivialDebug("There are " + allCops.Count + " cops within the clean area");

                if (availableCops.Count == 0)
                {
                    if (_settingsManager.CleanAreaSettings.EnableBackupUnit)
                    {
                        var closestRoad = RoadUtils.GetClosestRoad(playerPosition.Around(SpawnDistance), RoadType.All);

                        LspdfrUtils.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", playerPosition, true);
                        vehicle = _entityManager.CreateVehicleAt(closestRoad.Position, VehicleType.Police, 2);

                        vehicle.DriveToPosition(playerPosition, 30f, VehicleDrivingFlags.Normal, 5f)
                            .WaitForCompletion();
                        vehicle.EnableHazardIndicators();
                        vehicle.Occupants.ForEach(x => x.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion());
                        availableCops = vehicle.Occupants;
                    }
                    else
                    {
                        _rage.DisplayNotification("~b~Clear surrounding area~r~\nNo available cops within the surrounding area");
                        return;
                    }
                }

                foreach (var ped in availableCops)
                {
                    var duty = _dutyManager.NextAvailableDuty(ped, new List<DutyType>
                    {
                        DutyType.CleanCorpses,
                        DutyType.CleanWrecks
                    });

                    if (duty != null)
                    {
                        ped.CreateBlip();
                        _rage.LogTrivialDebug("Activating clear area duty " + duty);
                        duty.OnCompletion += (sender, args) =>
                        {
                            ped.DeleteBlip();
                            ped.ReturnToLspdfrDuty();
                        };
                    }
                    else
                    {
                        _rage.LogTrivialDebug("Couldn't find any available clear area duty for " + ped);

                        if (vehicle == null)
                            continue;

                        vehicle.DisableHazardLights();
                        vehicle.Wander();
                    }
                }
            }, "CleanAreaImpl");
        }

        #endregion
    }
}