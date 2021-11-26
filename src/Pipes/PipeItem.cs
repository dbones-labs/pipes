namespace Pipes
{
    using System;

    internal class PipeItem
    {
        public PipeItem(Type type)
        {
            Type = type;
            PipeItemType = PipeItemType.Type;
        }

        public PipeItem(object givenInstance)
        {
            GivenInstance = givenInstance;
            PipeItemType = PipeItemType.Instance;
        }


        public Type Type { get; }

        public object GivenInstance { get; }

        public PipeItemType PipeItemType { get; }
    }
}
