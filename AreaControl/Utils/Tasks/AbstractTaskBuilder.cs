using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public abstract class AbstractTaskBuilder<TBuilder> where TBuilder : AbstractTaskBuilder<TBuilder>
    {
        protected TaskIdentificationType _identificationType;
        protected TaskId _taskId = Tasks.TaskId.Unknown;
        protected TaskHash _taskHash = Tasks.TaskHash.Unknown;
        protected IEnumerable<Ped> _executorEntities;

        public TBuilder IdentificationType(TaskIdentificationType identificationType)
        {
            _identificationType = identificationType;
            return (TBuilder) this;
        }

        public TBuilder TaskId(TaskId taskId)
        {
            _taskId = taskId;
            return (TBuilder) this;
        }

        public TBuilder TaskHash(TaskHash taskHash)
        {
            _taskHash = taskHash;
            return (TBuilder) this;
        }

        public TBuilder ExecutorEntities(IEnumerable<Ped> executorEntities)
        {
            _executorEntities = executorEntities;
            return (TBuilder) this;
        }

        protected IEnumerable<ExecutorEntity> ConvertExecutorEntities()
        {
            return _executorEntities.Select(x => new ExecutorEntity(x)).ToList();
        }
    }
}