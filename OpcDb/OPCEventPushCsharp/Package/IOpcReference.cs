namespace Syncer.EventPush.Package
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// 获取到完整的OPC服务器tag的包
    /// </summary>
    public interface IOpcReference : IEventPack
    {
        /// <summary>
        /// tag的集合
        /// </summary>
        List<IOpcTag> OpcTagList { get; set; }
    }
}

