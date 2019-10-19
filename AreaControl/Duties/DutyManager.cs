using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Menu.Response;
using Rage;

namespace AreaControl.Duties
{
    public class DutyManager : IDutyManager, IDisposable
    {
        private readonly Dictionary<ACPed, List<IDuty>> _duties = new Dictionary<ACPed, List<IDuty>>();
        private readonly List<DutyListener> _dutyListeners = new List<DutyListener>();
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private bool _isActive = true;

        #region Constructors

        public DutyManager(IRage rage, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
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
        public long DutyId { get; private set; }

        /// <inheritdoc />
        public IDuty NextAvailableDuty(ACPed ped, IEnumerable<DutyType> dutyTypes)
        {
            Assert.NotNull(ped, "ped cannot be null");
            var position = ped.Instance.Position;
            var nextAvailableDuty = GetNextAvailableDuty(position, dutyTypes);

            if (nextAvailableDuty != null)
            {
                RegisterDuty(ped, nextAvailableDuty);
            }

            return nextAvailableDuty;
        }

        /// <inheritdoc />
        public IDuty NextAvailableOrIdleDuty(ACPed ped, IEnumerable<DutyType> dutyTypes)
        {
            Assert.NotNull(ped, "ped cannot be null");
            var nextAvailableDuty = NextAvailableDuty(ped, dutyTypes);

            if (nextAvailableDuty != null)
                return nextAvailableDuty;

            _rage.LogTrivialDebug("Their are no available duties in the area of " + ped.Instance.Position);
            
            return RegisterDuty(ped, GetIdleDuty());
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
        public IDuty NewRedirectTrafficDuty(ACPed ped, Vector3 position, float heading, ResponseCode responseCode)
        {
            return RegisterDuty(ped, new RedirectTrafficDuty(position, heading, responseCode));
        }

        /// <inheritdoc />
        public IDuty NewPlaceObjectsDuty(ACPed ped, IEnumerable<PlaceObjectsDuty.PlaceObject> objects, ResponseCode responseCode, bool placeFromHand)
        {
            return RegisterDuty(ped, new PlaceObjectsDuty(NextDutyId, objects, responseCode, placeFromHand));
        }

        /// <inheritdoc />
        public IDuty NewIdleDuty(ACPed ped)
        {
            return RegisterDuty(ped, GetIdleDuty());
        }

        /// <inheritdoc />
        public IDuty RegisterDuty(ACPed ped, IDuty duty)
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
            _rage.LogTrivialDebug("Registered duty " + duty.GetType().Name + " for ped " + ped);

            if (!HasActiveDuty(ped))
            {
                _rage.LogTrivialDebug("Ped has no activate duty, directly executing registered duty");
                ActivateNextDutyOnPed(ped);
            }

            return duty;
        }

        /// <inheritdoc />
        public void DismissDuties()
        {
            foreach (var pedDuties in _duties)
            {
                var duties = pedDuties.Value;

                foreach (var duty in duties.Where(x => x.State == DutyState.Active))
                {
                    duty.Abort();
                }

                duties.Clear();
                var dismissDuty = new ReturnToVehicleDuty();
                dismissDuty.OnCompletion += (sender, args) => pedDuties.Key.ReturnToLspdfrDuty();
                RegisterDuty(pedDuties.Key, dismissDuty);
            }

            _dutyListeners.Clear();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the next duty ID.
        /// </summary>
        private long NextDutyId => ++DutyId;

        #endregion

        #region Methods 

        /// <inheritdoc />
        public void Dispose()
        {
            _isActive = false;
            DismissDuties();
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _rage.NewSafeFiber(() =>
            {
                while (_isActive)
                {
                    foreach (var dutyListener in _dutyListeners)
                    {
                        var pedInstance = dutyListener.Ped.Instance;

                        // check if the ped instance is still valid and the duty listener has an event registered
                        if (!pedInstance.IsValid() || dutyListener.OnDutyAvailable == null)
                            continue;

                        var availableDuty = GetNextAvailableDuty(pedInstance.Position, dutyListener.DutyTypes);

                        if (availableDuty == null)
                            continue;

                        _logger.Debug("Found a new available duty " + availableDuty + " for listener " + dutyListener);
                        RegisterDuty(dutyListener.Ped, availableDuty);
                        dutyListener.HasNewDuty = true;
                        dutyListener.OnDutyAvailable.Invoke(this, new DutyAvailableEventArgs(availableDuty));
                    }
                    
                    // cleanup listeners
                    _dutyListeners.RemoveAll(x => x.Ped.IsInvalid || x.OnDutyAvailable == null || x.HasNewDuty);

                    foreach (var ped in _duties.Keys)
                    {
                        if (HasDutyAvailable(ped))
                            ActivateNextDutyOnPed(ped);
                    }

                    GameFiber.Sleep(1000);
                }
            }, "DutyManager");
        }

        private IDuty GetNextAvailableDuty(Vector3 position, IEnumerable<DutyType> dutyTypes)
        {
            var duties = new List<IDuty>();

            foreach (var dutyType in dutyTypes)
            {
                switch (dutyType)
                {
                    case DutyType.CleanCorpses:
                        duties.Add(new CleanCorpsesDuty(++DutyId, position));
                        break;
                    case DutyType.CleanWrecks:
                        duties.Add(new CleanWrecksDuty(++DutyId, position));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dutyTypes), "Duty type " + dutyType + " has not yet been added to GetNextAvailableDuty");
                }
            }

            return duties.FirstOrDefault(x => x.IsAvailable && IsInstantiationAllowed(x));
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

        private void ActivateNextDutyOnPed(ACPed ped)
        {
            if (HasActiveDuty(ped))
                return;

            var duty = _duties[ped].FirstOrDefault(x => x.State == DutyState.Ready);

            if (duty == null)
            {
                _rage.LogTrivialDebug("Cannot activate next duty on ped as no duties with state READY are available for " + ped);
                return;
            }

            duty.OnCompletion += (sender, args) =>
            {
                _rage.LogTrivialDebug("Completed duty (and activating next available duty) " + sender);
                ActivateNextDutyOnPed(ped);
            };
            _rage.LogTrivialDebug("Activating next duty " + duty);
            duty.Execute();
        }

        private bool HasActiveDuty(ACPed ped)
        {
            return _duties[ped].Any(x => x.State == DutyState.Active);
        }

        private bool HasDutyAvailable(ACPed ped)
        {
            return _duties[ped].Any(x => x.State == DutyState.Ready);
        }

        private static IDuty GetIdleDuty()
        {
            return new ReturnToVehicleDuty();
        }

        #endregion

        private class DutyListener : IDutyListener
        {
            /// <inheritdoc />
            public ACPed Ped { get; internal set; }

            /// <inheritdoc />
            public IList<DutyType> DutyTypes { get; } = new List<DutyType>();

            /// <inheritdoc />
            public EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }

            /// <summary>
            /// Get or set if this duty listener has a new duty.
            /// </summary>
            public bool HasNewDuty { get; set; }

            public override string ToString()
            {
                return $"{nameof(ACPed)}: {Ped}, {nameof(OnDutyAvailable)}: {OnDutyAvailable}";
            }
        }
    }
}