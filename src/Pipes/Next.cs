namespace Pipes
{
    using System.Threading.Tasks;

    /// <summary>
    /// Calls the next action in the chain.
    /// </summary>
    /// <typeparam name="T"><see cref="T"/></typeparam>
    /// <param name="context">the context which is passed between the actions</param>
    /// <returns></returns>
    public delegate Task Next<in T>(T context);
}