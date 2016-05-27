namespace Syncer.EventPush.Package
{
    using System;
    /// <summary>
    /// opc和tag
    /// </summary>
    public interface IOpcTag : ITagName
    {
        Type type { get; set; }
    }
}

