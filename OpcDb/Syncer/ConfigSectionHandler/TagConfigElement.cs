using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WOLEI.WanXiang.Model.ConfigSectionHandler
{
    /// <summary>
    /// tag
    /// </summary>
    public class TagConfigElement : ConfigurationElement, Ielement, Syncer.opc.ITag
    {
        static int _ClientHandle = 0;
        static object lockobj = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected object _value;

        public TagConfigElement() : base()
        {
            lock (lockobj)
            {
                this.ClientHandle = ++_ClientHandle;
            }
            PropertyChanged += (a, b) => { };
        }


        /// <summary>
        /// opc的path
        /// </summary>
        [ConfigurationProperty("path", IsRequired = true)]
        public string name
        {
            get
            { return (string)this["path"]; }
            set
            { this["path"] = value; }
        }

        /// <summary>
        /// 更新频率
        /// </summary>
        [ConfigurationProperty("updateRate", IsRequired = true)]
        [LongValidatorAttribute()]
        public Int64 updateRate
        {
            get
            { return (Int64)this["updateRate"]; }
            set
            { this["updateRate"] = value; }
        }

        /// <summary>
        /// 项的ClientHandle
        /// </summary>
        public int ClientHandle { get; set; }

        /// <summary>
        /// 梆定的ServerHandle，更新时使用
        /// </summary>
        public int ServerHandle { get; set; }

        /// <summary>
        /// 属性值改变事件
        /// </summary>
        /// <remarks>非线程安全</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        public void setValue<U>(U newvalue)
        {
            this._value = (object)newvalue;
        }

        public void setValue(object newvalue)
        {
            this.value = newvalue;
        }

        public U getvalue<U>()
        {
            return (U)this.value;
        }


        public Type valueType { get; set; }


        /// <summary>
        /// 值
        /// </summary>
        public object value
        {
            get
            {
                return _value;
            }
            protected set
            {
                bool ischanged = (value != null && !value.Equals(this._value) || (value == null && this._value != null)); //值是否发生改变
                this._value = value;
                if (ischanged)
                    PropertyChanged.BeginInvoke(this, new PropertyChangedEventArgs("value"), null, null);
            }
        }

        public static Type getOPCTypeByVarType(short Enumeration)
        {
            switch (Enumeration)
            {
                case 18:
                    return typeof(int);
                case 11:
                    return typeof(bool);
                case 4:
                    return typeof(float);
                default:
                    if ((int)Enumeration == 19)
                        return typeof(Int32);
                    log.FatalFormat("没有定义类型{0}", Enumeration);
                    Console.WriteLine("有未知的tag类型");
                    throw new NotImplementedException("未知类型");
            }
        }
       
    }
}
