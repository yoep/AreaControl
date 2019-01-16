using System;
using AreaControl.Instances;

namespace AreaControl.Duties
{
    public abstract class AbstractDuty : IDuty
    {
        private ACPed _ped;

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

        /// <inheritdoc />
        public virtual void Execute()
        {
            if (State != DutyState.Ready)
                throw new InvalidDutyStateException("Duty cannot be executed because it's in an invalid state", State);

            State = DutyState.Active;
        }

        /// <inheritdoc />
        public virtual void Abort()
        {
            if (State == DutyState.Completed)
                return;

            State = DutyState.Aborted;
        }

        public override string ToString()
        {
            return $"{nameof(IsAvailable)}: {IsAvailable}," + Environment.NewLine +
                   $"{nameof(IsRepeatable)}: {IsRepeatable}," + Environment.NewLine +
                   $"{nameof(IsMultipleInstancesAllowed)}: {IsMultipleInstancesAllowed}," + Environment.NewLine +
                   $"{nameof(State)}: {State}," + Environment.NewLine +
                   $"{nameof(Ped)}: {Ped}," + Environment.NewLine +
                   $"{nameof(OnCompletion)}: {OnCompletion}";
        }

        protected virtual void CompleteDuty()
        {
            if (State != DutyState.Ready)
                return;

            State = DutyState.Completed;
            OnCompletion?.Invoke(this, EventArgs.Empty);
        }
    }
}