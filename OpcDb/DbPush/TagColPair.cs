using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncer.EventAction
{
    /// <summary>
    /// 
    /// </summary>
    public class TagColPair
    {
        /// <summary>
        /// kep tag名
        /// </summary>
        [Key]
        [StringLength(50)]
        public string tagName { get; set; }

        
        /// <summary>
        /// 列名
        /// </summary>
        [Index]
        [StringLength(50)]
        public string colName { get; set; }
    }
}
