using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;

namespace AreaControl.Utils.Tasks
{
    public static class TaskUtil
    {
        /// <summary>
        /// Empty all occupants of the given vehicle.
        /// </summary>
        /// <param name="vehicle">Set the vehicle to empty.</param>
        public static TaskExecutor EmptyVehicle(Vehicle vehicle)
        {
            Assert.NotNull(vehicle, "vehicle cannot be null");
            var occupants = vehicle.Occupants.ToList();
            NativeFunction.Natives.TASK_EVERYONE_LEAVE_VEHICLE(vehicle);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_EVERYONE_LEAVE_VEHICLE)
                .ExecutorEntities(occupants)
                .Build();
        }

        /// <summary>
        /// Go to the given entity.
        /// The entity will stop when the duration has been exceeded.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="target">Set the target entity to go to.</param>
        /// <param name="speed">Set the speed of the action (this can be the walk or drive speed).</param>
        /// <param name="duration">Set the max. duration of the task.</param>
        public static TaskExecutor GoToEntity(Ped ped, Entity target, float speed, int duration = -1)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(target, "target cannot be null");
            NativeFunction.Natives.TASK_GO_TO_ENTITY(ped, target, duration, 2f, speed, 1073741824, 0);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_GOTO_ENTITY)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Look at the target entity.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="target">Set the target entity to look at.</param>
        /// <param name="duration">Set the task max. duration (default -1 = no duration).</param>
        /// <returns>Returns the task executor for this task.</returns>
        public static TaskExecutor LookAtEntity(Ped ped, Entity target, int duration = -1)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(target, "target cannot be null");
            NativeFunction.Natives.TASK_LOOK_AT_ENTITY(ped, target, duration, 2048, 3);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Id)
                .TaskId(TaskId.CTaskTriggerLookAt)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Get if the given task ID is active.
        /// </summary>
        /// <param name="ped">Set the ped to check the task status on.</param>
        /// <param name="taskId">Set the task id to check.</param>
        /// <returns>Returns true when task is active, else false</returns>
        public static bool IsTaskActive(Ped ped, int taskId)
        {
            Assert.NotNull(ped, "ped cannot be null");
            return NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, taskId);
        }

        /// <summary>
        /// Get the status of the script task.
        /// </summary>
        /// <param name="ped">Set the ped to get the script task status from.</param>
        /// <param name="taskHash">Set the task hash to check the status of.</param>
        /// <returns>Returns a task status number.</returns>
        public static int GetScriptTaskStatus(Ped ped, uint taskHash)
        {
            Assert.NotNull(ped, "ped cannot be null");
            return NativeFunction.Natives.GET_SCRIPT_TASK_STATUS<int>(ped, taskHash);
        }
    }
}