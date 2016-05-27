namespace Syncer.EventPush.Package
{
    using System;

    /// <summary>
    /// tag改变
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueChanged<T> :  ITagName where T : IComparable
    {
        /// <summary>
        /// 新的值
        /// </summary>
        T value { get; set; }
    }
}

