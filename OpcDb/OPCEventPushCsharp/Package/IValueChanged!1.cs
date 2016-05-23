namespace Syncer.EventPush.Package
{
    using System;

    public interface IValueChanged<T> : IEventPack, Ivalue where T : IComparable
    {
        T value { get; set; }
    }
}

