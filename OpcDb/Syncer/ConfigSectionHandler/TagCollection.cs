using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOLEI.WanXiang.Model.ConfigSectionHandler
{
    /// <summary>
    /// opctag集合
    /// </summary>
    public class TagCollection <T>: ConfigurationElementCollection 
        where T :ConfigurationElement, Ielement, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((T)element).name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public T this[int index]
        {
            get
            {
                return (T)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public T this[string Name]
        {
            get
            {
                return (T)BaseGet(Name);
            }
        }

        public int IndexOf(T station)
        {
            return BaseIndexOf(station);
        }

        public void Add(T station)
        {
            BaseAdd(station);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(T station)
        {
            if (BaseIndexOf(station) >= 0)
                BaseRemove(station.name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}
