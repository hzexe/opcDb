using Syncer.EventPush.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Syncer.opc.Pack
{
    /// <summary>
    /// tag类型包
    /// </summary>
    [DataContract]
    public class OpcTag : IOpcTag, Ivalue
    {
        /// <summary>
        /// 完整名称
        /// </summary>
        [DataMember]
        public string tagName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [DataMember]
        public Type type { get; set; }
    }
}
