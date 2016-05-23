using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncer.EventPush.Package;
using System.Runtime.Serialization;

namespace Syncer.opc.Pack
{
    /// <summary>
    /// 连接状态
    /// </summary>
   [DataContract]
    public class ConnctionChanged : EventPack, Syncer.EventPush.Package.IConnectionChanged
    {
        /// <summary>
        /// 连接状态
        /// </summary>
       [DataMember] public bool connected { get; set; }
    }
}
