using System;
using System.Collections.Generic;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public class RageTaskExecutor : TaskExecutor
    {
        internal RageTaskExecutor(IEnumerable<ExecutorEntity> executorEntities, Task task) :
            base(TaskIdentificationType.Rage, TaskId.Unknown, TaskHash.Unknown, executorEntities)
        {
            Task = task;
        }

        /// <summary>
        /// Get the Rage task.
        /// </summary>
        public Task Task { get; }
        
        protected override void Init()
        {
            GameFiber.StartNew(() =>
            {
                GameFiber.Yield();

                while (!IsAborted && Task.Ped.IsValid() && Task.IsActive)
                {
                    GameFiber.Yield();
                }

                if (!IsAborted)
                {
                    IsCompleted = true;
                    OnCompletion?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    OnAborted?.Invoke(this, EventArgs.Empty);
                }

                OnCompletionOrAborted?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    internal class RageTaskExecutorBuilder : AbstractTaskBuilder<RageTaskExecutorBuilder, RageTaskExecutor>
    {
        private Task _task;

        private RageTaskExecutorBuilder()
        {
        }

        public static RageTaskExecutorBuilder Builder()
        {
            return new RageTaskExecutorBuilder();
        }

        public RageTaskExecutorBuilder Task(Task task)
        {
            _task = task;
            return this;
        }

        /// <inheritdoc />
        public override RageTaskExecutor Build()
        {
            Assert.NotNull(_executorEntities, "executor entities have not been set");
            Assert.NotNull(_task, "task has not been set");
            return new RageTaskExecutor(ConvertExecutorEntities(), _task);
        }
    }
}