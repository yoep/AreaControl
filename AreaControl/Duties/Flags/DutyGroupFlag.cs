using System;
using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Duties.Flags
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DutyGroupFlag
    {
        Cops = 1, // 00000001
        Firemen = 2, // 00000010
        Medics = 4, // 00000100
        AllEmergency = 7, // 00000111
        Mobs = 8,  // 00001000
        All = 15 // 00001111
    }
}