using System;
using System.Collections.Generic;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public class AnimationTaskExecutor : TaskExecutor
    {
        internal AnimationTaskExecutor(TaskIdentificationType identificationType, TaskId taskId, TaskHash taskHash,
            IEnumerable<ExecutorEntity> executorEntities, AnimationDictionary animationDictionary, string animationName, AnimationTask rageTask)
            : base(identificationType, taskId, taskHash, executorEntities)
        {
            AnimationDictionary = animationDictionary;
            AnimationName = animationName;
            RageTask = rageTask;
        }

        /// <summary>
        /// Get the animation directory of this animation task.
        /// </summary>
        public AnimationDictionary AnimationDictionary { get; }

        /// <summary>
        /// Get the animation name that is being played in this task.
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        /// Get the Rage animation task.
        /// </summary>
        public AnimationTask RageTask { get; }

        /// <inheritdoc />
        public override void Abort()
        {
            if (IsCompleted)
                return;

            IsAborted = true;

            foreach (var entity in ExecutorEntities)
            {
                TaskUtil.StopAnimation(entity.Ped, AnimationDictionary.Name, AnimationName);
            }
        }

        protected override void Init()
        {
            GameFiber.StartNew(() =>
            {
                GameFiber.Yield();

                while (RageTask.Ped.IsValid() && RageTask.IsPlaying && !IsAborted)
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

    internal class AnimationTaskExecutorBuilder : AbstractTaskBuilder<AnimationTaskExecutorBuilder>
    {
        private AnimationDictionary _animationDictionary;
        private AnimationTask _rageAnimationTask;
        private string _animationName;

        private AnimationTaskExecutorBuilder()
        {
        }

        public static AnimationTaskExecutorBuilder Builder()
        {
            return new AnimationTaskExecutorBuilder();
        }

        public AnimationTaskExecutorBuilder AnimationDictionary(AnimationDictionary animationDictionary)
        {
            _animationDictionary = animationDictionary;
            return this;
        }

        public AnimationTaskExecutorBuilder AnimationName(string name)
        {
            _animationName = name;
            return this;
        }

        public AnimationTaskExecutorBuilder RageTask(AnimationTask rageAnimationTask)
        {
            _rageAnimationTask = rageAnimationTask;
            return this;
        }

        public AnimationTaskExecutor Build()
        {
            Assert.NotNull(_identificationType, "identification type has not been set");
            Assert.NotNull(_executorEntities, "executor entities have not been set");
            Assert.NotNull(_animationDictionary, "animationDictionary type has not been set");
            Assert.NotNull(_animationName, "animationName has not been set");
            Assert.NotNull(_rageAnimationTask, "rageAnimationTask has not been set");

            return new AnimationTaskExecutor(_identificationType, _taskId, _taskHash, ConvertExecutorEntities(), _animationDictionary, _animationName,
                _rageAnimationTask);
        }
    }
}