using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties.Flags;
using AreaControl.Instances;
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

        internal CleanWrecksDuty(long id, ACPed ped) 
            : base(id, ped)
        {
            _position = Ped.Instance.Position;
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

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.CleanWrecks;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.Cops | DutyGroupFlag.Firemen;

        #endregion

        #region AbstractDuty

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Logger.Info($"Executing clean wrecks duty #{Id}");
            var wrecks = GetWrecks();

            foreach (var wreck in wrecks)
            {
                Logger.Debug("Making wreck persistent...");
                //make the wreck persistent as the game likes to remove it suddenly making this duty crash :(
                wreck.IsPersistent = true;

                Logger.Debug("Going to wreck " + (wrecks.IndexOf(wreck) + 1) + " of " + wrecks.Count);
                var goToExecutor = _code == ResponseCode.Code2 ? Ped.WalkTo(wreck) : Ped.RunTo(wreck);

                goToExecutor
                    .WaitForAndExecute(taskExecutor =>
                    {
                        Logger.Trace("Completed go to wreck for task " + taskExecutor);
                        return AnimationUtils.IssueTicket(Ped);
                    }, 30000)
                    .WaitForAndExecute(taskExecutor =>
                    {
                        Logger.Trace("Completed write ticket at wreck " + taskExecutor);
                        Ped.DeleteAttachments();

                        Rage.LogTrivialDebug("Calling tow truck for " + wreck.Model.Name + "...");
                        _arrestManager.RequestTowTruck(wreck, false);

                        return AnimationUtils.TalkToRadio(Ped);
                    }, 5000)
                    .WaitForCompletion(3000);
                _entityManager.RegisterDisposedWreck(wreck);
            }

            Logger.Info($"Completed clean wrecks duty #{Id}");
        }

        #endregion

        #region Functions

        private bool IsWreckInRang()
        {
            return GetWrecks().Count > 0;
        }

        private IList<Vehicle> GetWrecks()
        {
            return VehicleQueryUtils
                .FindVehiclesWithin(_position, SearchRange)
                .Where(x => !_entityManager.DisposedWrecks.Contains(x))
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