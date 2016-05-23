namespace Syncer.EventPush.Package
{
    using System;

    public class SetValuePack<T> : Ivalue where T:IComparable
    {
        public virtual string tagName { get; set; }

        public T value { get; set; }
    }
}

