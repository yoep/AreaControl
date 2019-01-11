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
        /// Check if this task has been completed.
        /// </summary>
        public virtual bool IsCompleted { get; private set; }

        /// <summary>
        /// Check if the <see cref="WaitForCompletion"/> was aborted and the task execution timed out.
        /// </summary>
        public bool IsAborted { get; protected set; }

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

            //TODO: implement
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

        private void Init()
        {
            GameFiber.StartNew(() =>
            {
                while (!IsCompleted | !IsAborted)
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
                            throw new NotImplementedException("The TaskIdentificationType has not bee implemented yet");
                    }

                    if (ExecutorEntities.All(x => x.CompletedTask))
                    {
                        IsCompleted = true;
                        OnCompletion?.Invoke(this, EventArgs.Empty);
                    }

                    GameFiber.Sleep(50);
                }
            });
        }

        private void TaskIdCompletedForAllExecutorEntities()
        {
            foreach (var executorEntity in ExecutorEntities.Where(x => !x.CompletedTask))
            {
                executorEntity.CompletedTask = TaskUtil.IsTaskActive(executorEntity.Ped, (int) TaskId);
            }
        }

        private void TaskStatusCompletedForAllExecutorEntities()
        {
            foreach (var executorEntity in ExecutorEntities.Where(x => !x.CompletedTask))
            {
                var status = TaskUtil.GetScriptTaskStatus(executorEntity.Ped, (uint) TaskHash);
                executorEntity.CompletionStatus = status;

                //assume that if the status is 0, the task has been completed
                //set the task to completed when it gets the status TaskNotAssignedStatus
                if (status == 0 | status == ExecutorEntity.TaskNotAssignedStatus)
                    executorEntity.CompletedTask = true;

                //when in debug, check if the hash was correct or not
                //if not, try to figure out which one is correct by looping over all of them
                CheckForIncorrectHash(executorEntity);
            }
        }

        [Conditional("DEBUG")]
        private void CheckForIncorrectHash(ExecutorEntity executorEntity)
        {
            if (!executorEntity.IsIncorrectTaskHash)
                return;

            var rage = IoC.Instance.GetInstance<IRage>();

            foreach (var value in Enum.GetValues(typeof(TaskHash)))
            {
                var status = TaskUtil.GetScriptTaskStatus(executorEntity.Ped, (uint) value);

                if (status != ExecutorEntity.TaskNotAssignedStatus)
                    rage.LogTrivial("Task Hash suggestion is " + value + "(" + (uint) value + ") for original task hash " + TaskHash);
            }
        }
    }

    internal class TaskExecutorBuilder : AbstractTaskBuilder<TaskExecutorBuilder>
    {
        private TaskExecutorBuilder()
        {
        }

        public static TaskExecutorBuilder Builder()
        {
            return new TaskExecutorBuilder();
        }

        public TaskExecutor Build()
        {
            Assert.NotNull(_identificationType, "identification type has not been set");
            Assert.NotNull(_executorEntities, "executor entities have not been set");
            return new TaskExecutor(_identificationType, _taskId, _taskHash, ConvertExecutorEntities());
        }
    }
}