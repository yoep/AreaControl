using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu;
using Rage;

namespace AreaControl.Duties
{
    public class DutyManager : IDutyManager, IDisposable
    {
        private readonly Dictionary<ACPed, List<IDuty>> _duties = new Dictionary<ACPed, List<IDuty>>();
        private readonly List<DutyListener> _dutyListeners = new List<DutyListener>();
        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private bool _isActive = true;

        #region Constructors

        public DutyManager(IRage rage, IEntityManager entityManager, IResponseManager responseManager)
        {
            _rage = rage;
            _entityManager = entityManager;
            _responseManager = responseManager;
        }

        #endregion

        #region IDutyManager implementation

        /// <inheritdoc />
        public IDutyListener this[ACPed ped]
        {
            get
            {
                var listener = new DutyListener
                {
                    Ped = ped
                };
                _dutyListeners.Add(listener);
                _rage.LogTrivialDebug("Registering a new duty listener for " + ped);
                return listener;
            }
        }

        /// <inheritdoc />
        public IDuty NextAvailableOrIdleDuty(ACPed ped)
        {
            var position = ped.Instance.Position;
            var nextAvailableDuty = GetNextAvailableDuty(position);

            if (nextAvailableDuty != null)
            {
                RegisterDuty(ped, nextAvailableDuty);
                return nextAvailableDuty;
            }

            _rage.LogTrivialDebug("Their are no available duties in the area of " + position);
            var idleDuty = GetIdleDuty();
            RegisterDuty(ped, idleDuty);
            return idleDuty;
        }

        /// <inheritdoc />
        public IReadOnlyList<IDuty> RegisteredDuties
        {
            get
            {
                var duties = new List<IDuty>();

                foreach (var value in _duties.Values)
                {
                    duties.AddRange(value);
                }

                return duties;
            }
        }

        /// <inheritdoc />
        public void RegisterDuty(ACPed ped, IDuty duty)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(duty, "duty cannot be null");
            List<IDuty> pedDuties;

            if (_duties.ContainsKey(ped))
            {
                pedDuties = _duties[ped];
            }
            else
            {
                pedDuties = new List<IDuty>();
                _duties.Add(ped, pedDuties);
            }

            duty.Ped = ped;
            pedDuties.Add(duty);
        }

        /// <inheritdoc />
        public void DismissDuties()
        {
            foreach (var duties in _duties.Values)
            {
                foreach (var duty in duties.Where(x => x.State == DutyState.Active))
                {
                    duty.Abort();
                }

                duties.Clear();
            }

            _dutyListeners.Clear();
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            _isActive = false;
            DismissDuties();
        }

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _rage.NewSafeFiber(() =>
            {
                while (_isActive)
                {
                    var clonedDutyListeners = new List<DutyListener>(_dutyListeners);

                    foreach (var dutyListener in clonedDutyListeners)
                    {
                        var pedInstance = dutyListener.Ped.Instance;

                        //check if the ped instance is still valid
                        if (!pedInstance.IsValid())
                            continue;

                        var availableDuty = GetNextAvailableDuty(pedInstance.Position);

                        if (availableDuty == null || dutyListener.OnDutyAvailable == null)
                            continue;

                        _rage.LogTrivialDebug("Found a new available duty " + availableDuty + " for listener " + dutyListener);
                        RegisterDuty(dutyListener.Ped, availableDuty);
                        _dutyListeners.Remove(dutyListener);
                        dutyListener.OnDutyAvailable.Invoke(this, new DutyAvailableEventArgs(availableDuty));
                    }

                    GameFiber.Sleep(500);
                }
            }, "DutyManager");
        }

        private IDuty GetNextAvailableDuty(Vector3 position)
        {
            return GetDuties(position).FirstOrDefault(x => x.IsAvailable && IsInstantiationAllowed(x));
        }

        private bool IsInstantiationAllowed(IDuty duty)
        {
            if (duty.IsMultipleInstancesAllowed)
                return true;

            if (!duty.IsRepeatable && HasAlreadyBeenInstantiatedBefore(duty))
                return false;

            //check if the duty has never been instantiated before or not active anymore
            return RegisteredDuties.Where(x => x.GetType() == duty.GetType()).All(x => x.State != DutyState.Active);
        }

        private bool HasAlreadyBeenInstantiatedBefore(IDuty duty)
        {
            return RegisteredDuties.Any(x => x.GetType() == duty.GetType());
        }

        private IEnumerable<IDuty> GetDuties(Vector3 position)
        {
            return new List<IDuty>
            {
                new CleanCorpsesDuty(position, _responseManager.ResponseCode),
                new CleanWrecksDuty(position, _entityManager, _responseManager.ResponseCode)
            };
        }

        private static IDuty GetIdleDuty()
        {
            return new ReturnToVehicleDuty();
        }

        #endregion

        public class DutyListener : IDutyListener
        {
            /// <inheritdoc />
            public ACPed Ped { get; internal set; }

            /// <inheritdoc />
            public EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }

            public override string ToString()
            {
                return $"{nameof(ACPed)}: {Ped}, {nameof(OnDutyAvailable)}: {OnDutyAvailable}";
            }
        }
    }
}