namespace Pipes
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// this represent a middle ware pipeline (a series of <see cref="Action{T}"/>)
    /// </summary>
    /// <typeparam name="TContext">the context which the middleware will process</typeparam>
    public interface IMiddleware<in TContext>
    {
        Task Execute(IServiceProvider scope, TContext context);
    }
}