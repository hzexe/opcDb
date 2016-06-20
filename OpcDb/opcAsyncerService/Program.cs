using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace opcAsyncerService
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>

        static void Main()

        {
            Console.WriteLine("start!");
            if (System.Environment.GetCommandLineArgs().Contains("-s"))
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
            {
               new ServiceSync()
            };
                ServiceBase.Run(ServicesToRun);
            }
            else if (System.Environment.GetCommandLineArgs().Contains("-i"))
            {
                IDictionary mySavedState = new Hashtable();
                AssemblyInstaller ass = new AssemblyInstaller(typeof(ProjectInstaller).Assembly, new string[0]);


                //ass.UseNewContext = true;

                try

                {
                    ass.Install(mySavedState);
                    ass.Commit(mySavedState);
                    Console.WriteLine("安装成功");
                }
                catch (Exception ex)
                {
                    ass.Rollback(mySavedState);
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    ass.Dispose();

                }
            }
            else if (System.Environment.GetCommandLineArgs().Contains("-c"))
            {
                new ServiceSync().start();
                Console.WriteLine("已启动");
                var mc = System.Text.RegularExpressions.Regex.Match(System.Environment.CommandLine, @"-c (\d+)");
                int pid = 0;
                if (mc.Success)
                {
                    Console.WriteLine("当前程序作为pid:" + mc.Groups[1].Value + "的子程序进行，如果父进程关闭，本进程也会自动关闭");
                    pid = int.Parse(mc.Groups[1].Value);
                }
                Process pp = null;

                if (pid > 0)
                {
                    pp = Process.GetProcessById(pid);
                    pp.EnableRaisingEvents = true;
                    pp.Exited += (a, b) => { Process.GetCurrentProcess().Kill(); };

                }

                while (true)
                {
                    System.Threading.Thread.Sleep(2000);
                }
            }
            else
            {
                Console.WriteLine("使用方式:");
                Console.WriteLine("-i       安装成NT服务");
                Console.WriteLine("-c       控制台方式运行");
                Console.WriteLine("默认       显示帮助");

            }
        }
    }
}
