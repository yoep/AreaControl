using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AreaControl.AbstractionLayer;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public class TaskExecutor
    {
        internal TaskExecutor(TaskIdentificationType identificationType, TaskId taskId, TaskHash taskHash, IEnumerable<ExecutorEntity> executorEntities)
        {
            IdentificationType = identificationType;
            TaskId = taskId;
            TaskHash = taskHash;
            ExecutorEntities = executorEntities;
            
            Init();
        }

        /// <summary>
        /// Get the identification type of this task.
        /// </summary>
        public TaskIdentificationType IdentificationType { get; }

        /// <summary>
        /// Get the task id as identification.
        /// Can be null, check <see cref="IdentificationType"/> to determine what to use.
        /// </summary>
        public TaskId TaskId { get; }

        /// <summary>
        /// Get the task hash as identification.
        /// Can be null, check <see cref="IdentificationType"/> to determine what to use.
        /// </summary>
        public TaskHash TaskHash { get; }

        /// <summary>
        /// Get one or more peds which are executing this task.
        /// </summary>
        public IEnumerable<ExecutorEntity> ExecutorEntities { get; }

        /// <summary>
        /// Get the event handler for when this task executor is completed.
        /// You can register a new event here that will be triggered when the state changes to COMPLETED.
        /// </summary>
        public EventHandler OnCompletion { get; set; }

        /// <summary>
        /// Get the event handler for when this task executor is aborted.
        /// You can register a new event here that will be triggered when the state changes to ABORTED.
        /// </summary>
        public EventHandler OnAborted { get; set; }

        /// <summary>
        /// Get the event handler for when this task executor is completed or aborted.
        /// You can register a new event here that will be triggered when the state changes to COMPLETED or ABORTED.
        /// </summary>
        public EventHandler OnCompletionOrAborted { get; set; }

        /// <summary>
        /// Check if this task has been completed.
        /// </summary>
        public bool IsCompleted { get; protected set; }

        /// <summary>
        /// Check if the <see cref="WaitForCompletion"/> was aborted and the task execution timed out.
        /// </summary>
        public bool IsAborted { get; protected set; }

        /// <summary>
        /// Check if the this task executor is running.
        /// </summary>
        public bool IsRunning => !IsCompleted && !IsAborted;

        /// <summary>
        /// Get the parent executor that invoked this task executor.
        /// </summary>
        public TaskExecutor Parent { get; private set; }

        /// <summary>
        /// Wait for the task to be completed.
        /// </summary>
        /// <param name="duration">Set the max. duration of the task before aborted (-1 = no duration).</param>
        /// <returns>Returns this instance.</returns>
        public TaskExecutor WaitForCompletion(int duration = -1)
        {
            if (duration == -1)
                GameFiber.WaitUntil(() => IsCompleted | IsAborted);
            else
                GameFiber.WaitUntil(() => IsCompleted | IsAborted, duration);

            IsAborted = !IsCompleted;

            return this;
        }

        /// <summary>
        /// Wait for task completion and execute a new task.
        /// </summary>
        /// <param name="task">Set the next task to execute</param>
        /// <param name="duration">Set the max. duration of the task before aborted (-1 = no duration).</param>
        /// <returns>Returns the task executor of the new task.</returns>
        public TaskExecutor WaitForAndExecute(Func<TaskExecutor> task, int duration = -1)
        {
            WaitForCompletion(duration);
            var taskExecutor = task.Invoke();
            taskExecutor.Parent = this;
            return taskExecutor;
        }

        /// <summary>
        /// Wait for task completion and execute a new task.
        /// Gives this task executor as argument in the function (for logging purposes).
        /// </summary>
        /// <param name="task">Set the next task to execute</param>
        /// <param name="duration">Set the max. duration of the task before aborted (-1 = no duration).</param>
        /// <returns>Returns the task executor of the new task.</returns>
        public TaskExecutor WaitForAndExecute(Func<TaskExecutor, TaskExecutor> task, int duration = -1)
        {
            WaitForCompletion(duration);
            var taskExecutor = task.Invoke(this);
            taskExecutor.Parent = this;
            return taskExecutor;
        }

        /// <summary>
        /// Wait for task completion and execute the given action.
        /// </summary>
        /// <param name="action">Set the action to execute</param>
        /// <param name="duration">Set the max. duration of the task before aborted (-1 = no duration).</param>
        /// <returns>Returns the task executor of the new task.</returns>
        public TaskExecutor WaitForAndExecute(Action action, int duration = -1)
        {
            WaitForCompletion(duration);
            action.Invoke();
            return this;
        }

        /// <summary>
        /// Wait for task completion and execute the given action.
        /// Gives this task executor as argument in the function (for logging purposes).
        /// </summary>
        /// <param name="action">Set the action to execute</param>
        /// <param name="duration">Set the max. duration of the task before aborted (-1 = no duration).</param>
        /// <returns>Returns the task executor of the new task.</returns>
        public TaskExecutor WaitForAndExecute(Action<TaskExecutor> action, int duration = -1)
        {
            WaitForCompletion(duration);
            action.Invoke(this);
            return this;
        }

        /// <summary>
        /// Checks if the task has already been completed, if not, will abort the task execution.
        /// </summary>
        public virtual void Abort()
        {
            if (IsCompleted)
                return;

            IsAborted = true;
        }

        public override string ToString()
        {
            var message = $"{nameof(IdentificationType)}: {IdentificationType}," + Environment.NewLine +
                          $"{nameof(TaskId)}: {TaskId}," + Environment.NewLine +
                          $"{nameof(TaskHash)}: {TaskHash}," + Environment.NewLine +
                          $"{nameof(IsCompleted)}: {IsCompleted}" + Environment.NewLine +
                          $"{nameof(IsAborted)}: {IsAborted}" + Environment.NewLine +
                          $"---{nameof(ExecutorEntities)}:---";
            return ExecutorEntities.Aggregate(message, (current, entity) => current + Environment.NewLine + entity);
        }

        protected virtual void Init()
        {
            GameFiber.StartNew(() =>
            {
                // keep checking the state of the task when the task has not yet been completed or aborted
                while (!IsCompleted && !IsAborted)
                {
                    switch (IdentificationType)
                    {
                        case TaskIdentificationType.Id:
                            TaskIdCompletedForAllExecutorEntities();
                            break;
                        case TaskIdentificationType.Hash:
                            TaskStatusCompletedForAllExecutorEntities();
                            break;
                        case TaskIdentificationType.Animation:
                            //ignore this one as Rage will determine the state
                            break;
                        default:
                            throw new NotImplementedException("The TaskIdentificationType has not been implemented yet");
                    }

                    if (ExecutorEntities.All(x => x.CompletedTask))
                        IsCompleted = true;

                    GameFiber.Wait(100);
                }

                if (IsCompleted)
                    OnCompletion?.Invoke(this, EventArgs.Empty);
                if (IsAborted)
                    OnAborted?.Invoke(this, EventArgs.Empty);

                OnCompletionOrAborted?.Invoke(this, EventArgs.Empty);
            });
        }

        private void TaskIdCompletedForAllExecutorEntities()
        {
            foreach (var executorEntity in ExecutorEntities.Where(x => !x.CompletedTask))
            {
                executorEntity.CompletedTask = TaskUtils.IsTaskActive(executorEntity.Ped, (int) TaskId);
            }
        }

        private void TaskStatusCompletedForAllExecutorEntities()
        {
            foreach (var executorEntity in ExecutorEntities.Where(x => !x.CompletedTask))
            {
                var status = TaskUtils.GetScriptTaskStatus(executorEntity.Ped, (uint) TaskHash);
                var lastStatus = executorEntity.CompletionStatus;
                executorEntity.CompletionStatus = status;

                if (status == (int) TaskStatus.None)
                    executorEntity.CompletedTask = true;

                //when in debug, check if the hash was correct or not
                //if not, try to figure out which one is correct by looping over all of them
                if (lastStatus == ExecutorEntity.UnknownCompletionStatus && (status == (int) TaskStatus.NoTask || status == (int) TaskStatus.Unknown))
                    CheckForIncorrectHash(executorEntity);
            }
        }

        [Conditional("DEBUG")]
        private void CheckForIncorrectHash(ExecutorEntity executorEntity)
        {
            var rage = IoC.Instance.GetInstance<IRage>();

            foreach (var value in Enum.GetValues(typeof(TaskHash)))
            {
                var status = TaskUtils.GetScriptTaskStatus(executorEntity.Ped, (uint) value);

                if (status == (int) TaskStatus.None)
                    rage.LogTrivial("Task Hash suggestion is " + value + "(" + (uint) value + ") for original task hash " + TaskHash);
            }
        }
    }

    internal class TaskExecutorBuilder : AbstractTaskBuilder<TaskExecutorBuilder, TaskExecutor>
    {
        private TaskIdentificationType _identificationType;
        private TaskId _taskId = Tasks.TaskId.Unknown;
        private TaskHash _taskHash = Tasks.TaskHash.Unknown;

        private TaskExecutorBuilder()
        {
        }

        public static TaskExecutorBuilder Builder()
        {
            return new TaskExecutorBuilder();
        }

        public TaskExecutorBuilder IdentificationType(TaskIdentificationType identificationType)
        {
            _identificationType = identificationType;
            return this;
        }

        public TaskExecutorBuilder TaskId(TaskId taskId)
        {
            _taskId = taskId;
            return this;
        }

        public TaskExecutorBuilder TaskHash(TaskHash taskHash)
        {
            _taskHash = taskHash;
            return this;
        }

        /// <inheritdoc />
        public override TaskExecutor Build()
        {
            Assert.NotNull(_identificationType, "identification type has not been set");
            Assert.NotNull(_executorEntities, "executor entities have not been set");
            return new TaskExecutor(_identificationType, _taskId, _taskHash, ConvertExecutorEntities());
        }
    }
}