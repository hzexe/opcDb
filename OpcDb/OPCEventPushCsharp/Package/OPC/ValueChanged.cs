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
    /// tag改变
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ValueChanged<T> :  IValueChanged<T>, ITagName where T :IComparable
    {
        [DataMember]
        public string tagName { get; set; }

        [DataMember]
        public T value { get; set; }
    }
}
