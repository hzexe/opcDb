using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace opcAsyncerService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            string parameter = "-s";
            var assemblyPath = Context.Parameters["assemblypath"];
            assemblyPath = string.Format("\"{0}\" {1}", assemblyPath, parameter);
            Context.Parameters["assemblypath"] = assemblyPath;
            base.OnBeforeInstall(savedState);
        }
    }
}
