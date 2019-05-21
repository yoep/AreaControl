using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AreaControl.Instances;
using Rage;
using Rage.Native;

namespace AreaControl.Utils.Tasks
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class TaskUtils
    {
        /// <summary>
        /// Go straight to the given position.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="position">Set the position to go to.</param>
        /// <param name="heading">Set the heading to take when arriving.</param>
        /// <param name="speed">Set the speed of the ped.</param>
        /// <param name="timeout">Set the max. time the ped can take</param>
        /// <returns>Returns the task executor.</returns>
        public static TaskExecutor GoToNative(Ped ped, Vector3 position, float heading, MovementSpeed speed, int timeout = 30000)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(position, "position cannot be null");
            KeepTaskForStatusChecks(ped);
            NativeFunction.Natives.TASK_GO_STRAIGHT_TO_COORD(ped, position.X, position.Y, position.Z, speed.Value, timeout, heading, 0f);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_GO_STRAIGHT_TO_COORD)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Go to the given entity using GTA V native function.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="target">Set the target entity to go to.</param>
        /// <param name="speed">Set the speed of the ped.</param>
        /// <param name="acceptedDistance">Set the accepted distance from the target</param>
        /// <param name="timeout">Set the max. time the ped can take</param>
        /// <returns>Returns the task executor.</returns>
        public static TaskExecutor GoToNative(Ped ped, Entity target, MovementSpeed speed, float acceptedDistance, int timeout = 30000)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(target, "target cannot be null");
            KeepTaskForStatusChecks(ped);
            NativeFunction.Natives.TASK_GO_TO_ENTITY(ped, target, timeout, acceptedDistance, speed.Value, speed, 0);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_GOTO_ENTITY)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Go to the given entity using the Rage task invoker.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="target">Set the target entity to go to.</param>
        /// <param name="speed">Set the speed of the ped.</param>
        /// <param name="timeout">Set the max. time the ped can take</param>
        /// <returns>Returns the task executor.</returns>
        public static TaskExecutor GoToRage(Ped ped, Entity target, MovementSpeed speed, int timeout = 30000)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(target, "target cannot be null");
            var pedPosition = ped.Position;
            var targetPosition = target.Position;
            var heading = MathHelper.ConvertDirectionToHeading(pedPosition - targetPosition);

            return GoToRage(ped, targetPosition, heading, speed, 0f, timeout);
        }

        /// <summary>
        /// Go to the given entity using the Rage task invoker.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="position">Set the position to go to.</param>
        /// <param name="heading">Set the heading to take when arriving.</param>
        /// <param name="speed">Set the speed of the ped.</param>
        /// <param name="acceptedDistance">Set the accepted distance from the target (default = 0).</param>
        /// <param name="timeout">Set the max. time the ped can take.</param>
        /// <returns>Returns the task executor.</returns>
        public static TaskExecutor GoToRage(Ped ped, Vector3 position, float heading, MovementSpeed speed, float acceptedDistance = 0f, int timeout = 30000)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(position, "position cannot be null");
            var task = ped.Tasks.GoStraightToPosition(position, speed.Value, heading, acceptedDistance, timeout);

            return RageTaskExecutorBuilder.Builder()
                .Task(task)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Go to the given entity.
        /// The entity will stop when the duration has been exceeded.
        /// </summary>
        /// <param name="ped">Set the ped that needs to executed the task.</param>
        /// <param name="target">Set the target entity to go to.</param>
        /// <param name="speed">Set the speed.</param>
        /// <param name="duration">Set the max. duration of the task.</param>
        public static TaskExecutor GoToEntity(Ped ped, Entity target, MovementSpeed speed, int duration = -1)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(target, "target cannot be null");
            KeepTaskForStatusChecks(ped);
            NativeFunction.Natives.TASK_GO_TO_ENTITY(ped, target, duration, 2f, speed.Value, 1073741824, 0);

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
        /// Enter a vehicle.
        /// </summary>
        /// <param name="ped">Set the ped to execute the given action.</param>
        /// <param name="vehicle">Set the vehicle to enter.</param>
        /// <param name="seat">Set the seat to enter.</param>
        /// <param name="speed">Set the speed.</param>
        /// <returns>Returns the task executor for this task.</returns>
        public static TaskExecutor EnterVehicle(Ped ped, Vehicle vehicle, VehicleSeat seat, MovementSpeed speed)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.TASK_ENTER_VEHICLE(ped, vehicle, -1, (int) seat, speed.Value, 1, 0);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Id)
                .TaskId(TaskId.CTaskEnterVehicleSeat)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Leave the given vehicle.
        /// </summary>
        /// <param name="ped">Set the ped to execute the given action.</param>
        /// <param name="vehicle">Set the vehicle to leave.</param>
        /// <param name="leaveVehicleFlags">Set the task flags.</param>
        /// <returns>Returns the task executor for this task.</returns>
        public static TaskExecutor LeaveVehicle(Ped ped, Vehicle vehicle, LeaveVehicleFlags leaveVehicleFlags)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.TASK_LEAVE_VEHICLE(ped, vehicle, (int) leaveVehicleFlags);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_LEAVE_VEHICLE)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Drive the vehicle to the given position.
        /// </summary>
        /// <param name="ped">Set the driver.</param>
        /// <param name="vehicle">Set the vehicle to drive.</param>
        /// <param name="position">Set the target position.</param>
        /// <param name="speed">Set the speed to drive at.</param>
        /// <param name="drivingFlags">Set the driving style flags.</param>
        /// <param name="acceptedDistance">Set the accepted distance from the target position.</param>
        /// <returns>Returns the task executor for this task.</returns>
        public static TaskExecutor DriveToPosition(Ped ped, Vehicle vehicle, Vector3 position, float speed, VehicleDrivingFlags drivingFlags,
            float acceptedDistance)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.NotNull(vehicle, "vehicle cannot be null");
            NativeFunction.Natives.TASK_VEHICLE_DRIVE_TO_COORD(ped, vehicle, position.X, position.Y, position.Z, speed, 1f, vehicle.Model.Hash,
                (uint) drivingFlags, acceptedDistance, true);

            return TaskExecutorBuilder.Builder()
                .IdentificationType(TaskIdentificationType.Hash)
                .TaskHash(TaskHash.TASK_VEHICLE_DRIVE_TO_COORD)
                .ExecutorEntities(new List<Ped> {ped})
                .Build();
        }

        /// <summary>
        /// Play the given animation through Rage, but manage it in the custom task executor for more options.
        /// </summary>
        /// <param name="ped">Set the ped to execute the animation on.</param>
        /// <param name="animationDictionary">Set the animation dictionary to load.</param>
        /// <param name="animationName">Set the animation name to play from the dictionary.</param>
        /// <param name="animationFlags">Set the animation flags to use on the animation playback.</param>
        /// <returns>Returns the animation task executor.</returns>
        public static AnimationTaskExecutor PlayAnimation(Ped ped, string animationDictionary, string animationName, AnimationFlags animationFlags)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            var dictionary = new AnimationDictionary(animationDictionary);
            var rageAnimationTask = ped.Tasks.PlayAnimation(dictionary, animationName, 1.5f, animationFlags);

            return AnimationTaskExecutorBuilder.Builder()
                .ExecutorEntities(new List<Ped> {ped})
                .AnimationDictionary(dictionary)
                .AnimationName(animationName)
                .RageTask(rageAnimationTask)
                .Build();
        }

        /// <summary>
        /// Stop the given animation task on the ped.
        /// </summary>
        /// <param name="ped">Set the ped to stop the animation on.</param>
        /// <param name="animationDictionary">Set the animation dictionary.</param>
        /// <param name="animationName">Set the animation name to stop.</param>
        public static void StopAnimation(Ped ped, string animationDictionary, string animationName)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            NativeFunction.Natives.STOP_ANIM_TASK(ped, animationDictionary, animationName, 1f);
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

        private static void KeepTaskForStatusChecks(Ped ped)
        {
            NativeFunction.Natives.SET_PED_KEEP_TASK(ped, true);
        }
    }
}