﻿using Syncer.EventPush.Package;
using Syncer.opc.Pack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WOLEI.WanXiang.Model.ConfigSectionHandler;

namespace Syncer.opc
{
    public class Context
    {
        protected OPCAutomation.OPCServer _opcServer;
        protected OPCAutomation.OPCGroup _opcGroup;
        protected static OPCTagSection ss;
        /// <summary>
        /// 推送方式对象的集合
        /// </summary>
        protected static List<EventPush.IDataPush> EventPushList;
        protected delegate void eh(object sender, bool d);

        /// <summary>
        /// OPC服务连接成功或是失败抛出的事件
        /// </summary>
        protected event eh opcConnctionChangedEvent;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 更新到opc的事务集合
        /// </summary>
        private ConcurrentDictionary<int, OpcUpdateResult> updateTransactionIDDic = new ConcurrentDictionary<int, OpcUpdateResult>(6, 10000);
        /// <summary>
        /// 更新opc的状态
        /// </summary>
        private int transactionID = 0;


        static Context()
        {
            try
            {
                ss = ConfigurationManager.GetSection("opc.config") as OPCTagSection;
#if(!DEBUG)
                if (null == ss) throw new Exception("配置文件出错");
#endif
                var list = (from DataChangedHandlerElement n in ss.DataChangedHandlers select n).ToList();
                var fi = new System.IO.FileInfo(Assembly.GetCallingAssembly().Location);
                var dir = fi.Directory.FullName;
                EventPushList = new List<EventPush.IDataPush>(list.Count);
                //实例化事件通知插件对象
                list.ForEach(dh =>
                {
                    var assPath = System.IO.Path.Combine(dir, dh.name);
                    var ass = Assembly.LoadFile(assPath);
                    Type type = ass.GetType(dh.type);
                    var instance = (EventPush.IDataPush)ass.CreateInstance(dh.type, true);
                    if (null == instance)
                        throw new Exception("实例化配置文件中" + dh.type + "时失败");
                    instance.setArguments(dh.arguments);
                    EventPushList.Add(instance);
                });
            }
            catch (Exception ex)
            {
                log.Fatal("读取配置文件中OPC相关配置时出错", ex);
                Console.WriteLine("读取配置文件中OPC相关配置时出错" + ex.Message);
                System.Environment.Exit(-1);
            }
        }

        public Context()
        {
            //var type=Type.GetTypeFromProgID("OPC.Automation");
            //var type = Type.GetTypeFromCLSID(new Guid("28E68F9A-8D75-11D1-8DC3-3C302A000000"));
            //_opcServer = Activator.CreateInstance(type);
            _opcServer = new OPCAutomation.OPCServerClass();
            opcConnctionChangedEvent += Station_opcConnctionChangedEvent;
            /*
            if (!connectOPCAsync().Result)
            {
                throw new Exception("连接不上opc，请检查配置");
            }
            */
            connectOPCAsync();
            EventPushList.ForEach(pl => pl.setReNewValueCallBack(new Func<IValuesChanged<IComparable>, bool>(  clientChangedValue)));
        }

        /// <summary>
        /// 插件提交的更新opc服务器的回调
        /// </summary>
        /// <param name="vp">是实现ISetValuePack泛型的接口</param>
        /// <returns></returns>
        private bool clientChangedValue(Syncer.EventPush.Package.IValuesChanged<IComparable> vsc)
        {
            //log.DebugFormat("数据库值改变需要更改到opc,tagname是{0}个", vsc.values.Count);
            Dictionary<int, object> dic = new Dictionary<int, object>();
            vsc.values.ForEach(vp => {
                var tag = (from TagConfigElement item in ss.tags where item.name.Equals(vp.tagName) select item).FirstOrDefault();
                if (null == tag) return;
                //log.DebugFormat("数据库值改变需要更改到opc,tagname是{0},值是{1}", vp.tagName, vp.value);
                dic.Add(tag.ServerHandle, vp.value);
            });
            updateDataToOPCServer(dic);
            return true;
        }
        /// <summary>
        /// 处理要更新到server的tag数据
        /// </summary>
        /// <param name="dic">键是tag和ServerHandle,值是要更新的tag值的字典</param>
        protected void updateDataToOPCServer(IDictionary<int, object> dic)
        {
            if (null == dic || dic.Count == 0) return;
            var keylist = dic.Keys.ToList();
            keylist.Insert(0, 0);   //因为下标从1开始,开始插入空的
            var valuelist = dic.Values.ToList();
            valuelist.Insert(0, null);   //因为下标从1开始,开始插入空的

            var ArrServerHandle = (Array)keylist.ToArray();
            var ArrLocalValue = (Array)valuelist.ToArray();
            var ArrError = (Array)new Int32[dic.Count];
            int a, b;
            a = DateTime.Now.Millisecond;
            b = a;

            var opcresult = new OpcUpdateResult(new AutoResetEvent(false));
            this.updateTransactionIDDic.AddOrUpdate(transactionID, opcresult, (aa, bb) => opcresult);
            log.DebugFormat("批量更新OPC的值此次更新{0}条数据,更新编号是{1}", dic.Count, transactionID);
            _opcGroup.AsyncWrite(dic.Count, ref ArrServerHandle, ref ArrLocalValue, out ArrError, transactionID++, out b);
            //等待更新到opc服务器的更新结果,这里因受DCOM影响,最长时间可能需要6分钟才有结果
            //但中间有可能存在opc重连接的情况,不能死等因此需要设最大值6分钟,
            if (opcresult.autoEvent.WaitOne(TimeSpan.FromMinutes(0.2D)))
            {
                if (opcresult.result.All(x => x))
                {
                    log.DebugFormat("更新OPC的值更新编号{0},结果全部成功", transactionID);
                }
                else
                {
                    log.FatalFormat("更新OPC的值更新编号{0},有{1}项失败", transactionID, opcresult.result.Count(x => !x));
                }
            }else
            {
                log.FatalFormat("更新OPC的值更新编号{0},6分钟内结果无反馈", transactionID);
            }
            opcresult.Dispose();
        }


        /// <summary>
        /// 发送数据到OPCServer
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="tag">要发送的控件</param>
        /// <param name="newValue">新的值</param>
        [Obsolete("因为使用了批量更新,并且添加了更新的回调,此函数与其还会冲突",true)]
        protected void updateDataToOPCServer<T>(ITag tag, T newValue)
        {
            var ArrServerHandle = (Array)new int[] { 0, tag.ServerHandle };
            var ArrLocalValue = (Array)new object[] { default(T), newValue };
            var ArrError = (Array)new Int32[] { 0 };
            int a, b;
            a = DateTime.Now.Millisecond;
            b = a;
            _opcGroup.AsyncWrite(1, ref ArrServerHandle, ref ArrLocalValue, out ArrError, a, out b);
        }

        protected virtual void Station_opcConnctionChangedEvent(object sender, bool e)
        {
            log.Debug("Station_opcConnctionChangedEvent");
            {
                Pack.ConnctionChanged p = new Pack.ConnctionChanged() { connected = e };
                Parallel.ForEach(EventPushList, c => c.connectChanged(p));
            }

            if (e)
            {
                //OPC服务器连接成功时
                var groupName = "theopc_" + this.GetHashCode();
                // _opcServer.OPCGroups.Remove(groupName);

                _opcGroup = _opcServer.OPCGroups.Add(groupName);
                _opcGroup.AsyncReadComplete += _opcGroup_AsyncReadComplete;
                var test = _opcServer.OPCGroups;
                _opcGroup.DataChange += _opcGroup_DataChange;
                _opcGroup.AsyncWriteComplete += _opcGroup_AsyncWriteComplete;
                Array ClientHandles = new int[ss.tags.Count + 1]; //Array.CreateInstance
                Array arr_error = new int[ss.tags.Count + 1];
                {
                    int i = 1;  //下标要从1开始

                    foreach (TagConfigElement item in ss.tags)
                    {
                        OPCAutomation.OPCItem opcItem = _opcGroup.OPCItems.AddItem(item.name, item.ClientHandle);  //IOPCItem  OPCItem
                        item.valueType = TagConfigElement.getOPCTypeByVarType(opcItem.CanonicalDataType);
                        //28E68F99-8D75-11D1-8DC3-3C302A000000
                        item.ServerHandle = opcItem.ServerHandle;
                        opcItem.IsActive = true;
                        ClientHandles.SetValue(item.ServerHandle, i);
                        i++;
                    }
                    {
                        Pack.OpcReference p = new Pack.OpcReference() { };
                        p.OpcTagList = (from TagConfigElement item in ss.tags select new Pack.OpcTag() { tagName = item.name, type = item.valueType })
                           .ToList().
                           ConvertAll(c => c as IOpcTag);
                        Parallel.ForEach(EventPushList, c =>
                        {
                            try
                            {
                                c.opcReferenceReady(p);
                            }
                            catch (Exception ex1)
                            {
                                log.Warn("调用插件opcReferenceReady失败", ex1);
                            }

                        });
                        //EventPushList.ForEach(c => c.opcReferenceReady(p));
                    }
                }
                //_opcGroup.DeadBand=
                _opcGroup.UpdateRate = 600;
                _opcGroup.ClientHandle = this.GetHashCode();
                _opcGroup.IsSubscribed = true;
                _opcGroup.IsActive = true;
                log.Debug("_opcGroup reged");
                //AsyncRead(int NumItems, ref Array ServerHandles, out Array Errors, int TransactionID, out int CancelID);
                //int tid = DateTime.Now.Second;
                //int cid = 0;
                //_opcGroup.AsyncRead(ss.tags.Count, ref ClientHandles, out arr_error, tid, out cid);
            }
            else
            {
                //OPC服务器连接失败
                _opcGroup = null;
                System.Threading.Thread.Sleep(500);
                connectOPCAsync();  //继续连接
            }
        }

        private void _opcGroup_AsyncWriteComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array Errors)
        {
            bool[] result = new bool[NumItems]; //批量更新结果的数组
            //下标从1开始的
            for (int i = 1; i <= NumItems; i++)
            {
                result[i - 1] = 0 == (int)Errors.GetValue(i);
            }
            if (updateTransactionIDDic.ContainsKey(TransactionID))
            {
                OpcUpdateResult r;
                if (updateTransactionIDDic.TryRemove(TransactionID, out r))
                {
                    r.result = result;
                    try
                    {
                        r.autoEvent.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                        log.Warn("更新opc时超时的信号已销毁,不再可用");
                    }
                   
                }
                else
                {
                    updateTransactionIDDic[TransactionID].result = result;
                    try
                    {
                        updateTransactionIDDic[TransactionID].autoEvent.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                        log.Warn("更新opc时超时的信号已销毁,不再可用");
                    }
                }

            }
            else
            {

            }
        }

        protected virtual void _opcGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            
            if (NumItems == 0) return;
            ValuesChanged<IComparable> vc = new ValuesChanged<IComparable>();

            //特别注意，下标是1，而不是0
            for (int i = 1; i <= NumItems; i++)
            {
                //ClientHandles
                var clientHandle = (int)ClientHandles.GetValue(i);
                var value = ItemValues.GetValue(i); //更新的值
                if (null == value) continue;
                var opcitem = (from TagConfigElement item in ss.tags select item).Single(cuc => clientHandle == cuc.ClientHandle);
                opcitem.setValue(value);
                vc.values.Add(new ValueChanged<IComparable>() { tagName = opcitem.name, value = (IComparable)value });
            }
            Parallel.ForEach(EventPushList, c => c.OpcValuesChanged(vc));
            log.InfoFormat("_opcGroup_DataChange,opc值改变:{0}",vc);
        }

        protected virtual void _opcGroup_AsyncReadComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps, ref Array Errors)
        {
            //throw new NotImplementedException();
        }

        private Task<bool> connectOPCAsync()
        {
            log.Info("connectOPCAsync  start");

            var r = Task<bool>.Factory.StartNew(() =>
            {
                log.Debug("连接OPC线程ID：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                try
                {
                    _opcServer.Connect(ss.programName, ss.serverAddress);
                    //_opcServer.Connect("127.0.0.1", "Kepware.KEPServerEnterprise.V5");
                }
                catch (Exception ex)
                {
                    log.Warn("连接OPC服务器，抛出异常", ex);
                }
                log.Info("connectOPCAsync  end");
                var issuccess =((int)OPCAutomation. OPCServerState.OPCRunning).Equals(_opcServer.ServerState);

                if (issuccess)
                {
                    // 检测OPC连接状况
                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                var eefdf = _opcServer.CurrentTime;//连接断开时，这句会异常
                            }
                            catch (System.Runtime.InteropServices.COMException erx)
                            {
                                opcConnctionChangedEvent.BeginInvoke(this, false, null, null);
                                log.Warn("检测到与OPC的服务连接异常断开", erx);
                                break;
                            }
                            System.Threading.Thread.Sleep(500);
                        }
                    });
                    log.Debug("连接OPC服务器成功");
                }
                else
                {
                    log.Warn("连接OPC服务器失败");
                }
                opcConnctionChangedEvent.Invoke(this, issuccess);
                return issuccess;
            });
            return r;
        }
    }
}
