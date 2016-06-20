using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WOLEI.WanXiang.Model.ConfigSectionHandler;

namespace Syncer.opc
{
    public interface ITag : Ielement,INotifyPropertyChanged
    {
        /// <summary>
        /// 更新频率
        /// </summary>
        Int64 updateRate { get; set; }

        /// <summary>
        /// 项的ClientHandle
        /// </summary>
        int ClientHandle { get; set; }

        /// <summary>
        /// 梆定的ServerHandle，更新时使用
        /// </summary>
        int ServerHandle { get; set; }

        /// <summary>
        /// 通过泛型设置值
        /// </summary>
        /// <typeparam name="U">值类型</typeparam>
        /// <param name="newvalue">新的值</param>
        /// <remarks>确保类型是相同的，或是可以直接强转的，否则会出现运行时错误</remarks>
        void setValue<U>(U newvalue);
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="newvalue"></param>
        void setValue(object newvalue);

        /// <summary>
        /// 获取泛型的值
        /// </summary>
        /// <typeparam name="U">值的类型</typeparam>
        /// <returns>项的值</returns>
        /// <remarks>确保类型是相同的，或是可以直接强转的，否则会出现运行时错误</remarks>
        U getvalue<U>();
        /// <summary>
        /// 获取值
        /// </summary>
        object value
        {
            get;
        }

            /// <summary>
            /// 值的类型
            /// </summary>
            Type valueType { get;  }
    }

    public delegate void DIOPCGroupEvent_AsyncReadCompleteEventHandler(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps, ref Array Errors);
    public delegate void DIOPCGroupEvent_DataChangeEventHandler(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps);
    public delegate void DIOPCGroupEvent_AsyncWriteCompleteEventHandler(int TransactionID, int NumItems, ref Array ClientHandles, ref Array Errors);
    public delegate void DIOPCGroupEvent_AsyncCancelCompleteEventHandler(int CancelID);
}
