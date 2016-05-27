using Syncer.EventPush.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Syncer.opc.Pack
{
    /// <summary>
    /// 数据敢变的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ValuesChanged<T> : EventPack, IValuesChanged<T> where T : IComparable
    {
        public ValuesChanged() {
            values = new List<IValueChanged<T>>();
        }
        public ValuesChanged(int count)
        {
            values = new List<IValueChanged<T>>(count);
        }

        [DataMember]
        public List<IValueChanged<T>> values { get; set; }
    }
}
