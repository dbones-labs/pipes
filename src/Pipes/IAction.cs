namespace Pipes
{
    using System.Threading.Tasks;

    /// <summary>
    /// a single action within the pipeline chain
    /// </summary>
    /// <typeparam name="T">the context type</typeparam>
    public interface IAction<T>
    {
        /// <summary>
        /// a method in the chain which will process the context
        /// </summary>
        /// <param name="context">the context to process</param>
        /// <param name="next">the next action in the chain, execute this to continue the chain</param>
        /// <returns>let the caller know when your are done</returns>
        Task Execute(T context, Next<T> next);
    }
}