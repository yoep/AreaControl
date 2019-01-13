using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using AreaControl.Utils.Query;
using Arrest_Manager.API;
using Rage;

namespace AreaControl.Duties
{
    public class CleanWrecksDuty : IDuty
    {
        private const float SearchRange = 35f;
        private const string SpeedoModelName = "SPEEDO";

        private readonly Vector3 _position;
        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private ACPed _ped;

        internal CleanWrecksDuty(Vector3 position, IEntityManager entityManager)
        {
            _position = position;
            _entityManager = entityManager;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        public bool IsAvailable => IsWreckInRang();

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsRepeatable => true;
        
        /// <inheritdoc />
        public bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
            _ped = ped;
            _rage.NewSafeFiber(() =>
            {
                var wrecks = GetWrecks();

                _rage.LogTrivialDebug("Executing CleanWrecksDuty...");
                foreach (var wreck in wrecks)
                {
                    _rage.LogTrivialDebug("Going to wreck " + (wrecks.IndexOf(wreck) + 1) + " of " + wrecks.Count);
                    ped.WalkTo(wreck)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed walk to wreck for task " + taskExecutor);
                            return AnimationUtil.IssueTicket(ped);
                        }, 30000)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed write ticket at wreck " + taskExecutor);
                            ped.DeleteAttachments();
                            _rage.LogTrivialDebug("Calling tow truck for " + wreck.Model.Name + "...");
                            Functions.RequestTowTruck(wreck, false);
                            return AnimationUtil.TalkToRadio(ped);
                        }, 5000)
                        .WaitForCompletion(3000);
                    _entityManager.RegisterDisposedWreck(wreck);
                }

                _rage.LogTrivialDebug("CleanWrecksDuty completed");
                IsActive = false;
                OnCompletion?.Invoke(this, EventArgs.Empty);
            }, "CleanWrecksDuty.Execute");
        }

        /// <inheritdoc />
        public void Abort()
        {
            _ped.ActivateDuty(new ReturnToVehicleDuty());
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
                .Where(x => !IsSpeedoModel(x))
                .ToList();
        }

        private static bool IsSpeedoModel(IRenderable vehicle)
        {
            return vehicle.Model.Name.ToUpper().Equals(SpeedoModelName);
        }

        private static bool IsWreck(Vehicle vehicle)
        {
            return vehicle.IsEmpty && !vehicle.IsPoliceVehicle || vehicle.IsPoliceVehicle && !vehicle.IsDriveable;
        }

        #endregion
    }
}