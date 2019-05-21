using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.TrafficBreak
{
    public class TrafficBreakImpl : ITrafficBreak
    {
        private const float DistanceFromPlayer = 250f;
        private const string DispatchAudio = "WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION";

        private readonly IRage _rage;
        private readonly IResponseManager _responseManager;
        private readonly IEntityManager _entityManager;
        private readonly List<TaskExecutor> _taskExecutors = new List<TaskExecutor>();

        public TrafficBreakImpl(IRage rage, IResponseManager responseManager, IEntityManager entityManager)
        {
            _rage = rage;
            _responseManager = responseManager;
            _entityManager = entityManager;
        }

        #region ISlowDownTraffic

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        #endregion

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.SlowDownTraffic);
        
        /// <inheritdoc />
        public MenuType Type => MenuType.STREET_CONTROL;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
                return;

            IsActive = true;
            Execute();
        }

        /// <inheritdoc />
        public void OnMenuHighlighted(IMenu sender)
        {
            //no-op
        }

        #endregion

        private void Execute()
        {
            _rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                var closestRoad = RoadUtils.GetClosestRoad(position, RoadType.All);
                var closestLane = RoadUtils.GetClosestLane(closestRoad, position);
                var positionBehindPlayer = position + MathHelper.ConvertHeadingToDirection(RoadUtils.OppositeHeading(closestLane.Heading)) * DistanceFromPlayer;
                var roadBehindPlayer = RoadUtils.GetClosestRoad(positionBehindPlayer, RoadType.MajorRoadsOnly);

                _rage.DisplayNotification("Requesting dispatch to ~b~slow down traffic~s~ nearby " + World.GetStreetName(position) + "...");
                Functions.PlayScannerAudioUsingPosition(DispatchAudio + " " + _responseManager.ResponseCodeAudio, position);
                GameFiber.Sleep(5000);
                Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");

                var applicableLanesBehindPlayer = roadBehindPlayer.Lanes.Where(x => Math.Abs(x.Heading - closestLane.Heading) < 25f).ToList();
                var applicableLanesNearPlayer = closestRoad.Lanes.Where(x => Math.Abs(x.Heading - closestLane.Heading) < 1f).ToList();

                for (var i = 0; i < applicableLanesNearPlayer.Count; i++)
                {
                    var laneNearPlayer = applicableLanesNearPlayer[i];
                    var laneBehindPlayer = GetLaneFromIndex(applicableLanesBehindPlayer, i);

                    var vehicle = _entityManager.FindVehicleWithinOrCreateAt(laneBehindPlayer.Position, laneBehindPlayer.Position, 30f, 1);
                    vehicle.EnableSirens();

                    var executor = vehicle.DriveToPosition(laneNearPlayer.Position, 15f, VehicleDrivingFlags.Emergency, 10f);
                    executor.OnCompletionOrAborted += (sender, args) =>
                    {
                        vehicle.DisableSirens();
                        vehicle.Wander();
                    };
                    _taskExecutors.Add(executor);
                }

                while (_taskExecutors.Any(x => x.IsRunning))
                {
                    GameFiber.Sleep(250);
                }

                Functions.PlayScannerAudio("WE_ARE_CODE_4");
                IsActive = false;
                _entityManager.Dismiss();
            }, "SlowDownTraffic.Execute");
        }

        private static Road.Lane GetLaneFromIndex(IReadOnlyList<Road.Lane> applicableLanesBehindPlayer, int index)
        {
            return index > applicableLanesBehindPlayer.Count - 1
                ? applicableLanesBehindPlayer.Last()
                : applicableLanesBehindPlayer[index];
        }
    }
}