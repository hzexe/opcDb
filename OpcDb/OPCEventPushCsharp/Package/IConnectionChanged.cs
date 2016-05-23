namespace Syncer.EventPush.Package
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 连接OPC服务器状态改变的包
    /// </summary>
    public interface IConnectionChanged : IEventPack
    {
        /// <summary>
        /// 当前的连接状态
        /// </summary>
        bool connected { get;  set; }
    }
}

