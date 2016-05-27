namespace Syncer.EventPush
{
    using Syncer.EventPush.Package;
    using System;
    using System.ServiceModel;

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

        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void OpcValuesChanged<T>(IValuesChanged<T> cs) where T :IComparable;

        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void opcReferenceReady(IOpcReference reference);


        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void setArguments(string arguments);

        [OperationContract(IsOneWay = true, IsTerminating = false, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
        void setReNewValueCallBack<T>(Func<IValuesChanged<T>, bool> callback) where T : IComparable;
    }
}

