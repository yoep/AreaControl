using System.Collections.Generic;
using System.Linq;
using Rage;

namespace AreaControl.Utils.Tasks
{
    public abstract class AbstractTaskBuilder<TBuilder, TOut> where TBuilder : AbstractTaskBuilder<TBuilder, TOut>
    {
        protected IEnumerable<Ped> _executorEntities;

        public TBuilder ExecutorEntities(IEnumerable<Ped> executorEntities)
        {
            _executorEntities = executorEntities;
            return (TBuilder) this;
        }

        /// <summary>
        /// Build the task executor instance.
        /// </summary>
        /// <returns>Returns the <see cref="TOut"/> instance.</returns>
        public abstract TOut Build();

        protected IEnumerable<ExecutorEntity> ConvertExecutorEntities()
        {
            return _executorEntities.Select(x => new ExecutorEntity(x)).ToList();
        }
    }
}