using System;
using System.Threading;
using AreaControl.AbstractionLayer;
using AreaControl.Duties.Exceptions;
using AreaControl.Duties.Flags;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public abstract class AbstractDuty : IDuty
    {
        protected readonly IRage Rage = IoC.Instance.GetInstance<IRage>();
        protected readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();
        private IGameFiberWrapper _executionThread;

        #region Constructor

        protected AbstractDuty(long id, ACPed ped)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Id = id;
            Ped = ped;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public long Id { get; }

        /// <inheritdoc />
        public abstract bool IsAvailable { get; }

        /// <inheritdoc />
        public abstract bool IsRepeatable { get; }

        /// <inheritdoc />
        public abstract bool IsMultipleInstancesAllowed { get; }

        /// <inheritdoc />
        public DutyState State { get; protected set; } = DutyState.Initializing;

        /// <inheritdoc />
        public abstract DutyTypeFlag Type { get; }

        /// <inheritdoc />
        public abstract DutyGroupFlag Groups { get; }

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public ACPed Ped { get; }

        /// <summary>
        /// Get if this duty has been aborted.
        /// </summary>
        protected bool IsAborted => State == DutyState.Aborted;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Execute()
        {
            if (State != DutyState.Ready || State != DutyState.Interrupted)
                throw new InvalidDutyStateException("Duty cannot be executed because it's in an invalid state", State);

            State = DutyState.Active;
            Ped.IsBusy = true;

            _executionThread = Rage.NewSafeFiber(() =>
                {
                    try
                    {
                        DoExecute();
                        Complete();
                    }
                    catch (ThreadAbortException)
                    {
                        // no-op
                    }
                    catch (ThreadInterruptedException)
                    {
                        // no-op
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                        Abort(false);
                    }
                }, GetType().Name + ".ExecuteThread");
        }

        /// <inheritdoc />
        public virtual void Abort()
        {
            Abort(true);
        }

        /// <inheritdoc />
        public void AfterRegistration()
        {
            State = DutyState.Ready;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GetType().Name}" + Environment.NewLine +
                   $"{nameof(Id)}: {Id}," + Environment.NewLine +
                   $"{nameof(IsAvailable)}: {IsAvailable}," + Environment.NewLine +
                   $"{nameof(IsRepeatable)}: {IsRepeatable}," + Environment.NewLine +
                   $"{nameof(IsMultipleInstancesAllowed)}: {IsMultipleInstancesAllowed}," + Environment.NewLine +
                   $"{nameof(State)}: {State}" + Environment.NewLine +
                   $"--- {nameof(Ped)} ---" + Environment.NewLine +
                   Ped + Environment.NewLine +
                   "---";
        }

        #endregion

        #region Functions

        /// <summary>
        /// Execute the duty functionality.
        /// </summary>
        protected abstract void DoExecute();

        /// <summary>
        /// Complete this duty.
        /// This method will invoke the #OnCompletion event handler if present and update the ped & duty state. 
        /// </summary>
        protected virtual void Complete()
        {
            // check if the duty hasn't already been completed
            if (State != DutyState.Active)
                return;

            Ped.IsBusy = false;
            State = DutyState.Completed;
            OnCompletion?.Invoke(this, EventArgs.Empty);
        }

        private void Abort(bool terminateExecutionThread)
        {
            if (State == DutyState.Completed)
                return;

            // check if the execution thread needs to be aborted
            if (terminateExecutionThread)
                _executionThread?.Abort();

            State = DutyState.Aborted;
            Ped.IsBusy = false;
        }

        #endregion
    }
}