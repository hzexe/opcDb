using Syncer.EventPush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncer.EventPush.Package;
using System.Reflection.Emit;
using System.Data;
using System.Data.Entity;
using System.Reflection;
using System.Diagnostics;
using Syncer.opc.Pack;

namespace Syncer.EventAction
{
    public class DbPush : IDataPush
    {
        /// <summary>
        /// 数据连接参数
        /// </summary>
        string dbconfigname;
        /// <summary>
        /// 内存保存的数据记录
        /// </summary>
        /// <remarks>它只在opc值改变时才改变</remarks>
        object oldrecord;   //最近一次数据记录,用来比较数据是否发生改变
        /// <summary>
        /// 数据库值改变时执行的委托
        /// </summary>
        Func<IValuesChanged<IComparable>, bool> valueChangedCallback;
        object lockobj = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DbPush()
        {
            //定时轮询数据值是否发生改变
            System.Timers.Timer tmr = new System.Timers.Timer(500);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Start();
        }

        /// <summary>
        /// 获取plc_value记录的第一个对象值
        /// </summary>
        /// <param name="dbset"></param>
        /// <returns></returns>
        public object getFirstRecord()
        {
            MysqlContext db = new MysqlContext(dbconfigname);
            var x = db.Set(MysqlContext.plcvalue).SqlQuery("select * from plc_value").GetEnumerator();
            x.MoveNext();
            object r = x.Current;
            db.Dispose();
            return r;
        }


        /// <summary>
        /// 获取值是否改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Debug.WriteLine("Tmr_Elapsed");

            if (null == oldrecord || null == valueChangedCallback) return;
            MysqlContext db = new MysqlContext(dbconfigname);
            var ps = MysqlContext.plcvalue.GetProperties(); //所有属性
            Dictionary<string, IComparable> changeddic = new Dictionary<string, IComparable>();   //改变的值的字典
            object newobject = null;
            try
            {
                newobject = getFirstRecord();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return;
            }
            finally
            {
                db.Dispose();
            }

            lock (lockobj)
            {
                ps.ToList().ForEach(p =>
                {
                    if (!p.Name.Equals("id"))
                    {
                        var q = new FastReflection.FastProperty(p);
                        var v1 = (IComparable)q.Get(newobject);
                        var v2 = (IComparable)q.Get(oldrecord);
                        if ((null==v1&&null!= v2) ||  (null != v1 && v1.CompareTo(v2) != 0))
                        {
                            changeddic.Add(p.Name, v1);
                        }
                        else
                        {
                            // log.DebugFormat("检测值是否改变{0}")
                        }
                    }
                });
            }
            if (changeddic.Count > 0)
            {
                IValuesChanged<IComparable> ivc = new ValuesChanged<IComparable>(changeddic.Count);
                changeddic.ToList().ForEach(kv =>
                {
                        //var instance = (EventPush.Package.ITagName)Activator.CreateInstance(typeof(SetValuePack<>).MakeGenericType(kv.Value.GetType()));
                        db = new MysqlContext(dbconfigname);
                    try
                    {
                        IValueChanged<IComparable> vc = new ValueChanged<IComparable>();
                        vc.tagName = db.TagColPairs.First(t => t.colName.Equals(kv.Key)).tagName;
                        vc.value = kv.Value;
                        ivc.values.Add(vc);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("数据库值改变,创建改变包时失败", ex);
                        return;
                    }
                    finally
                    {
                        db.Dispose();
                    }

                });
                log.DebugFormat("有{0}项值的改变,改变的包为:{1}", changeddic.Count,ivc.ToString());
                valueChangedCallback.Invoke(ivc);
            }
        }

        public void connectChanged(IConnectionChanged desc)
        {
            //什么也不做
        }

        public void opcReferenceReady(IOpcReference reference)
        {
            Console.WriteLine("opcReferenceReady");

            TypeBuilder builder = DynamicType.CreateTypeBuilder(
"EF_DynamicModelAssembly",
"DynamicModule",
"plc_value");
            DynamicType.CreateAutoImplementedProperty(builder, "id", typeof(int));  //添加主键

            //TagColPair
            List<TagColPair> tplist = new List<TagColPair>(reference.OpcTagList.Count);
            for (int i = 0; i < reference.OpcTagList.Count; i++)
            {
                var conname = string.Format("item_{0}", i);
                var x = new TagColPair() { tagName = reference.OpcTagList[i].tagName, colName = conname };
                tplist.Add(x);
                //添加业务数据属性
                //判断类型是否是可为空的
                //value.GetType().isi
                //Nullable
                Type ptype = reference.OpcTagList[i].type;
                /*
                if (!ptype.IsClass && !ptype.IsInterface)
                {
                    //说明是值类型,设为可为空的
                    ptype = typeof(Nullable<>).MakeGenericType(reference.OpcTagList[i].type);
                }
                */
                DynamicType.CreateAutoImplementedProperty(builder, conname, ptype);
            }
            MysqlContext.plcvalue = builder.CreateType();       //动态entity类型
            MysqlContext.initeddata = tplist;                   //对应的表
            MysqlContext db = new MysqlContext(dbconfigname);
            try
            {
                oldrecord = getFirstRecord();
            }
            catch (Exception ex)
            {
                log.Warn("数据变化 操作数据库失败", ex);
                Console.WriteLine("数据变化 操作数据库失败");
            }
            finally
            {
                db.Dispose();
            }
        }

        public void setArguments(string arguments)
        {
            dbconfigname = arguments;
        }

        public void setReNewValueCallBack<T>(Func<IValuesChanged<T>, bool> callback) where T : IComparable
        {
            this.valueChangedCallback = (Func<IValuesChanged<IComparable>, bool>)callback;
        }

        public void OpcValuesChanged<T>(IValuesChanged<T> cs) where T : IComparable
        {
            if (null == cs || cs.values == null || cs.values.Count == 0) return;
            MysqlContext db = new MysqlContext(dbconfigname);
            MySql.Data.MySqlClient.MySqlParameter[] pars = new MySql.Data.MySqlClient.MySqlParameter[cs.values.Count];
            StringBuilder sb = new StringBuilder("update plc_value set ");
            try
            {
                for (int i = 0; i < cs.values.Count; i++)
                {
                    var x = cs.values[i];
                    var colname = db.TagColPairs.First(tcp => tcp.tagName.Equals(x.tagName)).colName;
                    var par = "@p" + i;
                    sb.AppendFormat("{0}={1},", colname, par);
                    pars[i] = new MySql.Data.MySqlClient.MySqlParameter(par, x.value);
                }
                sb.Remove(sb.Length - 1, 1);    //去掉最后的逗号
                string sql = sb.ToString();

                int changed;
                lock (lockobj)
                {
                    changed = db.Database.ExecuteSqlCommand(sql, pars);
                    oldrecord = getFirstRecord();
                }
                Debug.WriteLine("更新了" + changed);
            }
            catch (Exception ex)
            {
                log.Warn("OpcValuesChanged<>opc发生改变时,更新库失败", ex);
            }
            finally
            {
                db.Dispose();
            }
        }
    }
}
