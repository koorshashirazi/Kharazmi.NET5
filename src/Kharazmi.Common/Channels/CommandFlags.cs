using System;

namespace Kharazmi.Common.Channels
{
    [Flags]
    public enum CommandFlags
    {
        None = 0,
        FireAndForget = 2,
        PreferMaster = 0,
        DemandMaster = 4,
        PreferReplica = 8,
        DemandReplica = PreferReplica | DemandMaster,
        NoRedirect = 64,
        NoScriptCache = 512
    }
}