namespace Pipes.Tests.Contexts
{
    using System.Threading.Tasks;

    public interface IConsumer<in T>
    {
        Task Handle(T message);
    }
}