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

namespace Syncer.EventAction
{
    public class DbPush : IDataPush
    {
        string dbconfigname;
        object oldrecord;
        Func<Ivalue, bool> valueChangedCallback;
        object lockobj = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DbPush()
        {
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
            Dictionary<string, object> changeddic = new Dictionary<string, object>();   //改变的值的字典
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


            ps.ToList().ForEach(p =>
            {
                if (!p.Name.Equals("id"))
                {
                    var q = new FastReflection.FastProperty(p);
                    var v1 = q.Get(newobject);
                    var v2 = q.Get(oldrecord);
                    if ((null != v1 && !v1.Equals(v2)) || (null == v1 && null != v2) || !v1.Equals(v2))
                    {
                        changeddic.Add(p.Name, v1);
                        
                    }
                    else
                    {
                       // log.DebugFormat("检测值是否改变{0}")
                    }
                }
            });
            if (changeddic.Count > 0)
            {
                log.DebugFormat("有{0}项值的改变", changeddic.Count);
                lock (lockobj)
                {
                    oldrecord = newobject;
                    changeddic.ToList().ForEach(kv =>
                    {
                        var instance = (EventPush.Package.Ivalue)Activator.CreateInstance(typeof(SetValuePack<>).MakeGenericType(kv.Value.GetType()));
                        db = new MysqlContext(dbconfigname);
                        try
                        {
                            instance.tagName = db.TagColPairs.First(t => t.colName.Equals(kv.Key)).tagName;
                            new FastReflection.FastProperty(instance.GetType().GetProperty("value")).Set(instance, kv.Value);
                        }
                        catch
                        {
                            return;
                        }
                        finally
                        {
                            db.Dispose();
                        }
                        valueChangedCallback.Invoke(instance);
                    });
                }//end lock
            }
        }

        public void connectChanged(IConnectionChanged desc)
        {
            //什么也不做
        }

        public void itemDataChanged(IEventPack newValue)
        {
            //newValue 是个 IValueChanged 的泛型对象,同时也实现Ivalue
            MysqlContext db = new MysqlContext(dbconfigname);
            string tagname = ((Ivalue)newValue).tagName;
            try
            {
                //获得列名
                var colname = db.TagColPairs.First(tcp => tcp.tagName.Equals(tagname)).colName;
                PropertyInfo p = MysqlContext.plcvalue.GetProperty(colname);    //entity类型
                PropertyInfo p_v = newValue.GetType().GetProperty("value");     //泛型的
                object v = new FastReflection.FastProperty(p_v).Get(newValue);// p_v.GetValue(newValue); //值
                int changed;
                lock (lockobj)
                {
                    changed=db.Database.ExecuteSqlCommand("update plc_value set "+ colname + "=@a", 
                        new MySql.Data.MySqlClient.MySqlParameter("@a", v));

                    oldrecord = getFirstRecord();
                    //new FastReflection.FastProperty(p).Set(oldrecord, v);
                    //changed = db.SaveChanges();
                }
                Debug.WriteLine("更新了" + changed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                db.Dispose();
            }
        }

        public void opcReferenceReady(IOpcReference reference)
        {
            Console.WriteLine("opcReferenceReady");

            TypeBuilder builder = DynamicType.CreateTypeBuilder(
"EF_DynamicModelAssembly",
"DynamicModule",
"plc_value");

            //Type[] types = new Type[] { typeof(System.ComponentModel.DataAnnotations.KeyAttribute) };
            //Type[][] ts = new Type[][] { types };

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
                if (!ptype.IsClass && !ptype.IsInterface)
                {
                    //说明是值类型,设为可为空的
                    ptype = typeof(Nullable<>).MakeGenericType(reference.OpcTagList[i].type);
                }
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
                log.Warn("数据变化 操作数据库失败",ex);
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

        public void setReNewValueCallBack(Func<Ivalue, bool> callback)
        {
            this.valueChangedCallback = callback;
        }
    }
}
