using System.Collections.Generic;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public class AnimationTaskExecutor : TaskExecutor
    {
        internal AnimationTaskExecutor(TaskIdentificationType identificationType, TaskId taskId, TaskHash taskHash,
            IEnumerable<ExecutorEntity> executorEntities, AnimationDictionary animationDictionary, string animationName)
            : base(identificationType, taskId, taskHash, executorEntities)
        {
            AnimationDictionary = animationDictionary;
            AnimationName = animationName;
        }

        /// <summary>
        /// Get the animation directory of this animation task.
        /// </summary>
        public AnimationDictionary AnimationDictionary { get; }

        /// <summary>
        /// Get the animation name that is being played in this task.
        /// </summary>
        public string AnimationName { get; }
    }

    internal class AnimationTaskExecutorBuilder : AbstractTaskBuilder<AnimationTaskExecutorBuilder>
    {
        private AnimationDictionary _animationDictionary;
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

        public AnimationTaskExecutor Build()
        {
            Assert.NotNull(_identificationType, "identification type has not been set");
            Assert.NotNull(_executorEntities, "executor entities have not been set");
            Assert.NotNull(_animationDictionary, "animationDictionary type has not been set");
            Assert.NotNull(_animationName, "animationName has not been set");

            return new AnimationTaskExecutor(_identificationType, _taskId, _taskHash, ConvertExecutorEntities(), _animationDictionary, _animationName);
        }
    }
}