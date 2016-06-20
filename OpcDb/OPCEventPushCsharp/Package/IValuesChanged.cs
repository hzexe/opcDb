namespace Syncer.EventPush.Package
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 数据更改的包
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValuesChanged<T> : IEventPack where T :IComparable
    {
        List<IValueChanged<T>> values { get; set; }
    }
}

