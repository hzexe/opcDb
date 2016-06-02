using System;
using System.Threading;

namespace Syncer.opc
{
    /// <summary>
    /// 更新opc操作结果的封装
    /// </summary>
    internal class OpcUpdateResult : IDisposable
    {
        /// <summary>
        /// 操作结果
        /// </summary>
        public bool[] result { get; set; }
        /// <summary>
        /// 通知对象
        /// </summary>
        public AutoResetEvent autoEvent { get; set; }

        public OpcUpdateResult(AutoResetEvent autoEvent)
        {
            this.autoEvent = autoEvent;
        }

        public void Dispose()
        {
            autoEvent.Dispose();
        }
    }
}
