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
    /// opcReference
    /// </summary>
    [DataContract]
    [KnownTypeAttribute(typeof(OpcTag))]
    public class OpcReference : EventPack, IOpcReference
    {
        [DataMember]
        public List<IOpcTag> OpcTagList { get; set; }
    }
}
