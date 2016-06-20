namespace Syncer.EventPush
{
    using Syncer.EventPush.Package;
    using System;
    using System.ServiceModel;
    /// <summary>
    /// 插件接口
    /// </summary>
    [ServiceContract(SessionMode =System.ServiceModel.SessionMode.Allowed,ProtectionLevel =System.Net.Security.ProtectionLevel.None)]
    public interface IDataPush
    {
        /// <summary>
        /// 与opc服务器连接状态改变
        /// </summary>
        /// <param name="desc"></param>
        [OperationContract(IsOneWay =true,IsTerminating =false,ProtectionLevel =System.Net.Security.ProtectionLevel.None)]
        void connectChanged(IConnectionChanged desc);

        /*
        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void itemDataChanged(IEventPack newValue);
        */
        /// <summary>
        /// opc值改变的包
        /// </summary>
        /// <typeparam name="T">改变值的类型</typeparam>
        /// <param name="cs">数据更改的包</param>
        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void OpcValuesChanged<T>(IValuesChanged<T> cs) where T :IComparable;
        /// <summary>
        /// 获取到opc所有项类型
        /// </summary>
        /// <param name="reference"类型包></param>
        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void opcReferenceReady(IOpcReference reference);

        /// <summary>
        /// 设置初始化插件所需要的参数
        /// </summary>
        /// <param name="arguments"></param>
        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void setArguments(string arguments);
        /// <summary>
        /// 检测到需要发送到OPC命令的包封装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">回调给opc对象的值改变包</param>
        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void setReNewValueCallBack<T>(Func<IValuesChanged<T>, bool> callback) where T : IComparable;
    }
}

