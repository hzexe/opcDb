namespace Syncer.EventPush.Package
{
    using System;
    /// <summary>
    /// opc和tag
    /// </summary>
    public interface IOpcTag : Ivalue
    {
        Type type { get; set; }
    }
}

