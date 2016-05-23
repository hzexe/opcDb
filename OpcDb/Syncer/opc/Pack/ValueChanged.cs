using Syncer.EventPush.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Syncer.opc.Pack
{
    [DataContract]
    public class ValueChanged<T> : EventPack, IValueChanged<T>, Ivalue
    {
        [DataMember]
        public string tagName { get; set; }

        [DataMember]
        public T value { get; set; }
    }
}
