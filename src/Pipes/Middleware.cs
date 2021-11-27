namespace Pipes
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// this is the default Middleware chain, which will execute a series of <see cref="Action{T}"/>
    /// </summary>
    /// <typeparam name="TContext">the context which the pipeline will be executed with</typeparam>
    public class Middleware<TContext> : IMiddleware<TContext>
    {
        private readonly List<PipeItem> _pipedTypes = new();

        public virtual void Add<T>() where T : IAction<TContext>
        {
            _pipedTypes.Add(new PipeItem(typeof(T)));
        }

        public virtual void Add(Type actionType)
        {
            _pipedTypes.Add(new PipeItem(actionType));
        }

        public virtual void Use(Func<TContext, Next<TContext>, Task> action)
        {
            _pipedTypes.Add(new PipeItem(new ActionWrapper<TContext>(action)));
        }


        public virtual async Task Execute(IServiceProvider scope, TContext context)
        {
            var enumerator = _pipedTypes.GetEnumerator();

            var factory = new GetNextFactory<TContext>(enumerator, scope);
            await factory.GetNext()(context);
        }
    }

    
    /// <summary>
    /// same as the <see cref="Middleware{TContext}"/>, however each execute will be in its own Ioc scope./>
    /// </summary>
    /// <typeparam name="TContext">the type which the middleware is for</typeparam>
    public class ScopedMiddleware<TContext> : Middleware<TContext>
    {
        public override Task Execute(IServiceProvider scope, TContext context)
        {
            using var childScope = scope.CreateScope();
            return base.Execute(childScope.ServiceProvider, context);
        }
    }
}