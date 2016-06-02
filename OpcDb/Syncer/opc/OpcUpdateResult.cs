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
        /// 更新opc的结果
        /// </summary>
        public bool[] result { get; set; }
        /// <summary>
        /// 通知对象
        /// </summary>
        public AutoResetEvent autoEvent { get; set; }
        /// <summary>
        /// 初始化更新opc操作结果的封装
        /// </summary>
        /// <param name="autoEvent">opc更新结果的通知对象</param>
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
