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
        /// <summary>
        /// 已重写
        /// </summary>
        /// <returns>序列化成json的字符串</returns>
        public override string ToString()
        {
            //    Newtonsoft.Json
            // Newtonsoft.Json.
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
