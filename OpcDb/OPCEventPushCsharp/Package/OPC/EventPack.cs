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
    public abstract class EventPack : IEventPack
    {
        static int _ClientHandle = 0;
        static object lockobj = new object();

        protected object _value;

        public EventPack()
        {
            lock (lockobj)
            {
                this.EventId = ++_ClientHandle;
            }
        }
        [DataMember]
        public int EventId { get; set; }
    }
}
