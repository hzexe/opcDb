using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using MySql.Data.Entity;
using System.Diagnostics;

namespace Syncer.EventAction
{

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    class MysqlContext : DbContext
    {
        /// <summary>
        /// 数据表的类型
        /// </summary>
        public static Type plcvalue { get; set; }
        public static IEnumerable<TagColPair> initeddata { get; set; }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        static MysqlContext()
        {
            DbConfiguration.SetConfiguration(new MySqlEFConfiguration());
            //SetSqlGenerator("MySql.Data.MySqlClient", new MyOwnMySqlMigrationSqlGenerator());
            Database.SetInitializer(new MysqlCreateDatabaseIfNotExists());
            //Database.SetInitializer(new DropCreateDatabaseAlways<MysqlContext>());
        }

        public MysqlContext(string dbConnctionName)
            : base("name=" + dbConnctionName)
        {
#if (DEBUG)
            // Database.Log = s => Debug.WriteLine(s);
#endif
            //Configuration.AutoDetectChangesEnabled = false;

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.RegisterEntityType(plcvalue);
            base.OnModelCreating(modelBuilder);
        }
        /// <summary>
        /// tag和列的对应
        /// </summary>
        public virtual DbSet<TagColPair> TagColPairs { get; set; }
    }


    internal sealed class MysqlCreateDatabaseIfNotExists : CreateDatabaseIfNotExists<MysqlContext>
    {
        protected override void Seed(MysqlContext context)
        {
            if (null != MysqlContext.initeddata)
            {
                context.TagColPairs.AddRange(MysqlContext.initeddata);
            }


            //初始化默认数据
            var a = Activator.CreateInstance(MysqlContext.plcvalue);
            context.Set(MysqlContext.plcvalue).Add(a);

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
