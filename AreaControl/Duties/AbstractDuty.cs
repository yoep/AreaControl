using System;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public abstract class AbstractDuty : IDuty
    {
        private ACPed _ped;

        /// <inheritdoc />
        public long Id { get; protected set; }

        /// <inheritdoc />
        public abstract bool IsAvailable { get; }

        /// <inheritdoc />
        public abstract bool IsRepeatable { get; }

        /// <inheritdoc />
        public abstract bool IsMultipleInstancesAllowed { get; }

        /// <inheritdoc />
        public DutyState State { get; protected set; } = DutyState.Initializing;

        /// <inheritdoc />
        public EventHandler OnCompletion { get; set; }

        /// <inheritdoc />
        public virtual ACPed Ped
        {
            get { return _ped; }
            set
            {
                _ped = value;
                State = DutyState.Ready;
            }
        }

        /// <summary>
        /// Get if this duty has been aborted.
        /// </summary>
        protected bool IsAborted => State == DutyState.Aborted;

        /// <inheritdoc />
        public virtual void Execute()
        {
            if (State != DutyState.Ready)
                throw new InvalidDutyStateException("Duty cannot be executed because it's in an invalid state", State);

            State = DutyState.Active;
            Ped.IsBusy = true;
        }

        /// <inheritdoc />
        public virtual void Abort()
        {
            if (State == DutyState.Completed)
                return;

            State = DutyState.Aborted;
            Ped.IsBusy = false;
        }

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

        protected virtual void CompleteDuty()
        {
            if (State != DutyState.Active)
                return;

            Ped.IsBusy = false;
            State = DutyState.Completed;
            OnCompletion?.Invoke(this, EventArgs.Empty);
        }
    }
}