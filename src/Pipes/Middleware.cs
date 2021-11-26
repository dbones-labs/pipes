namespace Pipes
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// this is the default Middleware chain, which will execute a series of <see cref="Action{T}"/>
    /// </summary>
    /// <typeparam name="TContext">the context which the pipeline will be executed with</typeparam>
    public class Middleware<TContext> : IMiddleware<TContext>
    {
        private readonly List<PipeItem> _pipedTypes = new List<PipeItem>();

        public void Add<T>() where T : IAction<TContext>
        {
            _pipedTypes.Add(new PipeItem(typeof(T)));
        }

        public void Add(Type actionType)
        {
            _pipedTypes.Add(new PipeItem(actionType));
        }

        public void Use(Func<TContext, Next<TContext>, Task> action)
        {
            _pipedTypes.Add(new PipeItem(new ActionWrapper<TContext>(action)));
        }


        public async Task Execute(IServiceProvider scope, TContext context)
        {
            var enumerator = _pipedTypes.GetEnumerator();

            var factory = new GetNextFactory<TContext>(enumerator, scope);
            await factory.GetNext()(context);
        }
    }
}