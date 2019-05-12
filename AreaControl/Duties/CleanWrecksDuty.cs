using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using AreaControl.Utils;
using AreaControl.Utils.Query;
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
        private readonly IEntityManager _entityManager;
        private readonly IArrestManager _arrestManager;
        private readonly ResponseCode _code;

        internal CleanWrecksDuty(long id, Vector3 position)
        {
            Id = id;
            _position = position;
            _entityManager = IoC.Instance.GetInstance<IEntityManager>();
            _arrestManager = IoC.Instance.GetInstance<IArrestManager>();
            _code = IoC.Instance.GetInstance<IResponseManager>().ResponseCode;
        }

        #region IDuty

        /// <inheritdoc />
        public override bool IsAvailable => IsWreckInRang();

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Rage.NewSafeFiber(() =>
            {
                var wrecks = GetWrecks();

                Rage.LogTrivialDebug("Executing CleanWrecksDuty...");
                foreach (var wreck in wrecks)
                {
                    Rage.LogTrivialDebug("Making wreck persistent...");
                    //make the wreck persistent as the game likes to remove it suddenly making this duty crash :(
                    wreck.IsPersistent = true;

                    Rage.LogTrivialDebug("Going to wreck " + (wrecks.IndexOf(wreck) + 1) + " of " + wrecks.Count);
                    var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(wreck) : Ped.RunTo(wreck);

                    goToExecutor
                        .WaitForAndExecute(taskExecutor =>
                        {
                            Rage.LogTrivialDebug("Completed go to wreck for task " + taskExecutor);
                            return AnimationUtil.IssueTicket(Ped);
                        }, 30000)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            Rage.LogTrivialDebug("Completed write ticket at wreck " + taskExecutor);
                            Ped.DeleteAttachments();

                            Rage.LogTrivialDebug("Calling tow truck for " + wreck.Model.Name + "...");
                            _arrestManager.RequestTowTruck(wreck, false);

                            return AnimationUtil.TalkToRadio(Ped);
                        }, 5000)
                        .WaitForCompletion(3000);
                    _entityManager.RegisterDisposedWreck(wreck);
                }

                Rage.LogTrivialDebug("CleanWrecksDuty completed");
                CompleteDuty();
            }, "CleanWrecksDuty.Execute");
        }

        #endregion

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
            return vehicle.IsEmpty && vehicle.IsEngineOn && !vehicle.IsPoliceVehicle || (vehicle.IsPoliceVehicle && !vehicle.IsDriveable);
        }

        #endregion
    }
}