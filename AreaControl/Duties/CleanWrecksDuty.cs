using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using Arrest_Manager.API;
using Rage;

namespace AreaControl.Duties
{
    public class CleanWrecksDuty : AbstractDuty
    {
        private const float SearchRange = 50f;
        
        private static readonly List<string> ModelsBannedFromCleaning = new List<string>
        {
            "SPEEDO",
            "AMBULANCE",
            "FIRETRUCK"
        };

        private readonly Vector3 _position;
        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly ResponseCode _code;

        internal CleanWrecksDuty(long id, Vector3 position, IEntityManager entityManager, ResponseCode code)
        {
            Id = id;
            _position = position;
            _entityManager = entityManager;
            _code = code;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        public override bool IsAvailable => IsWreckInRang();

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public override void Execute()
        {
            base.Execute();

            _rage.NewSafeFiber(() =>
            {
                var wrecks = GetWrecks();

                _rage.LogTrivialDebug("Executing CleanWrecksDuty...");
                foreach (var wreck in wrecks)
                {
                    _rage.LogTrivialDebug("Making wreck persistent...");
                    //make the wreck persistent as the game likes to remove it suddenly making this duty crash :(
                    wreck.IsPersistent = true;

                    _rage.LogTrivialDebug("Going to wreck " + (wrecks.IndexOf(wreck) + 1) + " of " + wrecks.Count);
                    var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(wreck) : Ped.RunTo(wreck);

                    goToExecutor
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed go to wreck for task " + taskExecutor);
                            return AnimationUtil.IssueTicket(Ped);
                        }, 30000)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed write ticket at wreck " + taskExecutor);
                            Ped.DeleteAttachments();
                            _rage.LogTrivialDebug("Calling tow truck for " + wreck.Model.Name + "...");
                            Functions.RequestTowTruck(wreck, false);
                            return AnimationUtil.TalkToRadio(Ped);
                        }, 5000)
                        .WaitForCompletion(3000);
                    _entityManager.RegisterDisposedWreck(wreck);
                }

                _rage.LogTrivialDebug("CleanWrecksDuty completed");
                CompleteDuty();
            }, "CleanWrecksDuty.Execute");
        }

        #region Functions

        private bool IsWreckInRang()
        {
            return GetWrecks().Count > 0;
        }

        private IList<Vehicle> GetWrecks()
        {
            return VehicleQuery
                .FindVehiclesWithin(_position, SearchRange)
                .Where(x => !_entityManager.GetAllDisposedWrecks().Contains(x))
                .Where(IsWreck)
                .Where(x => !IsBannedModel(x))
                .ToList();
        }

        private static bool IsBannedModel(IRenderable vehicle)
        {
            var vehicleModel = vehicle.Model.Name.ToUpper();
            return ModelsBannedFromCleaning.Contains(vehicleModel);
        }

        private static bool IsWreck(Vehicle vehicle)
        {
            return vehicle.IsEmpty && !vehicle.IsPoliceVehicle || vehicle.IsPoliceVehicle && !vehicle.IsDriveable;
        }

        #endregion
    }
}