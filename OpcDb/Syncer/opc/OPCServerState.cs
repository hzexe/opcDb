using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncer.opc
{
    internal enum OPCServerState
    {
        OPCRunning = 1,
        OPCFailed = 2,
        OPCNoconfig = 3,
        OPCSuspended = 4,
        OPCTest = 5,
        OPCDisconnected = 6,
    }
}
