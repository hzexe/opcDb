namespace Syncer.EventPush.Package
{
    using System;
    /// <summary>
    /// 包的接口
    /// </summary>
    public interface IEventPack
    {
        /// <summary>
        /// 包序号
        /// </summary>
        int EventId { get; set; }
    }
}

