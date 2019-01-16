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
        private readonly List<IDuty> _duties = new List<IDuty>();
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
        public IDutyListener this[Vector3 position]
        {
            get
            {
                var listener = new DutyListener
                {
                    Position = position
                };
                _dutyListeners.Add(listener);
                return listener;
            }
        }

        /// <inheritdoc />
        public IDuty NextAvailableOrIdleDuty(Vector3 position)
        {
            var nextAvailableDuty = GetNextAvailableDuty(position);

            if (nextAvailableDuty != null)
            {
                RegisterDuty(nextAvailableDuty);
                return nextAvailableDuty;
            }

            _rage.LogTrivialDebug("Their are no available duties in the area of " + position);
            return GetIdleDuty();
        }

        /// <inheritdoc />
        public void RegisterDuty(IDuty duty)
        {
            Assert.NotNull(duty, "duty cannot be null");
            _duties.Add(duty);
        }

        /// <inheritdoc />
        public void DismissDuties()
        {
            foreach (var duty in _duties.Where(x => x.IsActive))
            {
                duty.Abort();
            }

            _duties.Clear();
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
                        var availableDuty = GetNextAvailableDuty(dutyListener.Position);

                        if (availableDuty == null || dutyListener.OnDutyAvailable == null)
                            continue;

                        _rage.LogTrivialDebug("Found a new available duty " + availableDuty + " for listener " + dutyListener);
                        RegisterDuty(availableDuty);
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
            return !_duties
                .Where(x => x.GetType() == duty.GetType())
                .Any(x => x.IsActive);
        }

        private bool HasAlreadyBeenInstantiatedBefore(IDuty duty)
        {
            return _duties.Any(x => x.GetType() == duty.GetType());
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
            public Vector3 Position { get; internal set; }

            /// <inheritdoc />
            public EventHandler<DutyAvailableEventArgs> OnDutyAvailable { get; set; }

            public override string ToString()
            {
                return $"{nameof(Position)}: {Position}, {nameof(OnDutyAvailable)}: {OnDutyAvailable}";
            }
        }
    }
}