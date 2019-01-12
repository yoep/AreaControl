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
        private ACPed _ped;

        internal CleanWrecksDuty(Vector3 position)
        {
            _position = position;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        /// <inheritdoc />
        public bool IsAvailable => IsWreckInRang();

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public bool IsRepeatable => false;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
            _ped = ped;
            _rage.NewSafeFiber(() =>
            {
                _rage.LogTrivialDebug("Executing CleanWrecksDuty...");
                foreach (var wreck in GetWrecks())
                {
                    ped.WalkTo(wreck)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed walk to wreck for task " + taskExecutor);
                            AnimationUtil.IssueTicket(ped);
                        }, 5000)
                        .WaitForAndExecute(taskExecutor =>
                        {
                            _rage.LogTrivialDebug("Completed write ticket at wreck " + taskExecutor);
                            Functions.RequestTowTruck(wreck, false);
                            return AnimationUtil.TalkToRadio(ped);
                        })
                        .WaitForCompletion(3000);
                }

                _rage.LogTrivialDebug("CleanWrecksDuty completed");
                IsActive = false;
                OnCompletion?.Invoke(this, EventArgs.Empty);
            }, "CleanWrecksDuty.Execute");
        }

        /// <inheritdoc />
        public void Abort()
        {
            _rage.NewSafeFiber(() =>
            {
                _rage.LogTrivialDebug("Ped is entering last vehicle for CleanCorpsesDuty");
                _ped.EnterLastVehicle(MovementSpeed.Walk)
                    .WaitForAndExecute(() => _ped.ReturnToLspdfrDuty());
                _rage.LogTrivialDebug("Ped should have been returned to LSPDFR duty for CleanCorpsesDuty");
            }, "CleanWrecksDuty.Abort");
        }

        #region Functions

        private bool IsWreckInRang()
        {
            return GetWrecks().Count > 0;
        }

        private ICollection<Vehicle> GetWrecks()
        {
            return VehicleQuery
                .FindVehiclesWithin(_position, SearchRange)
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