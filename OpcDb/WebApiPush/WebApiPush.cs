using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncer.EventPush.Package;
using Syncer.EventPush;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Syncer.EventAction
{
    /// <summary>
    /// 向webapi服务端发送数据
    /// </summary>
    public sealed class WebApiPush : IDataPush
    {
        Uri url;
        /// <summary>
        /// 要提交的
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="pack">要发送的包</param>
        /// <returns></returns>
        private Task< System.Net.Http.HttpResponseMessage> post(string method,IEventPack pack) {
           
            var uri=new Uri( url, method);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(pack);
            var x = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            return  client.PostAsync(uri, x);
        }

        public void connectChanged(IConnectionChanged desc)
        {
            post("connectChanged", desc);
        }

        public void itemDataChanged(IEventPack newValue)
        {
            post("itemDataChanged", newValue);
        }

 
        public void opcReferenceReady(IOpcReference reference)
        {
            post("opcReferenceReady", reference);
        }

        public void setArguments(string arguments)
        {
            this.url = new Uri( arguments);
        }

        public void setReNewValueCallBack(Func<ITagName,bool> callback) 
        {
            //设置的，这个不去实现

        }
    }
}
