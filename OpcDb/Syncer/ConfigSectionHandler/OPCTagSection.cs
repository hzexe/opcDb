using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace WOLEI.WanXiang.Model.ConfigSectionHandler
{
    public sealed class OPCTagSection : System.Configuration.ConfigurationSection
    {
        /// <summary>
        /// opc服务器地址
        /// </summary>
        [ConfigurationProperty("serverAddress", IsRequired = true)]
        public string serverAddress
        {
            get
            { return (string)this["serverAddress"]; }
            set
            { this["serverAddress"] = value; }
        }

        /// <summary>
        /// opc服务器地址
        /// </summary>
        [ConfigurationProperty("programName", IsRequired = true)]
        public string programName
        {
            get
            { return (string)this["programName"]; }
            set
            { this["programName"] = value; }
        }



        /**
         * 验证的类
IntegerValidatorAttribute
LongValidatorAttribute
RegexStringValidatorAttribute
StringValidatorAttribute
TimeSpanValidatorAttribute
         * 
         * * 
         * 
         * */
        [ConfigurationProperty("Tags", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(TagCollection<TagConfigElement>),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public TagCollection<TagConfigElement> tags
        {
            get
            {
                return (TagCollection<TagConfigElement>)base["Tags"];
            }
        }

        [ConfigurationProperty("DataChangedHandlers", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(TagCollection<DataChangedHandlerElement>),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public TagCollection<DataChangedHandlerElement> DataChangedHandlers
        {
            get
            {
                return (TagCollection<DataChangedHandlerElement>)base["DataChangedHandlers"];
            }
        }




    }
}
