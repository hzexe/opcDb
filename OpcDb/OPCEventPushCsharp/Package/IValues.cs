namespace Syncer.EventPush.Package
{
    using System;
    using System.Collections.Generic;

    public interface IValues : IEventPack
    {
        List<Ivalue> values { get; set; }
    }
}

