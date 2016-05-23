using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace opcAsyncerService
{
    partial class ServiceSync : ServiceBase
    {
        public ServiceSync()
        {
            InitializeComponent();
        }

        public void start() {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Syncer.opc.Context c = new Syncer.opc.Context();
        }

        public void stop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
        }
    }
}
