namespace Pipes.Tests.Fake3rdPartyFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// consider something like EasyNetQ
    /// </summary>
    public class FakeBus
    {
        readonly Dictionary<string, Func<object, Task>> _handlers = new Dictionary<string, Func<object, Task>>();

        /// <summary>
        /// register a handler method, which will be executed when a message arrives (imagine it)
        /// </summary>
        public void Subscribe<T>(string topic, Func<T, Task> handle) where T : class
        {
            _handlers.Add(topic, msg => handle((T)msg));
        }

        /// <summary>
        /// executes the register handler method, note there is no middleware in this class
        /// </summary>
        public async Task Handle<T>(string topic, T message)
        {
            await _handlers[topic](message);
        }
    }
}