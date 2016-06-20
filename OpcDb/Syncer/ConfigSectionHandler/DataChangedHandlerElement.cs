using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WOLEI.WanXiang.Model.ConfigSectionHandler
{
    /// <summary>
    /// tag
    /// </summary>
    public class DataChangedHandlerElement : ConfigurationElement, Ielement
    {
        /// <summary>
        /// filename
        /// </summary>
        [ConfigurationProperty("filename", IsRequired = true)]
        public string name
        {
            get
            { return (string)this["filename"]; }
            set
            { this["filename"] = value; }
        }

        /// <summary>
        /// 类型
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public String type
        {
            get
            { return (String)this["type"]; }
            set
            { this["type"] = value; }
        }

        /// <summary>
        /// json参数
        /// </summary>
        [ConfigurationProperty("arguments", IsRequired = true)]
        public string arguments
        {
            get
            { return (string)this["arguments"]; }
            set
            { this["arguments"] = value; }
        }
        //argument

    }
}
