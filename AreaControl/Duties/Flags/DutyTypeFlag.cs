using System;
using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Duties.Flags
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DutyTypeFlag
    {
        None = 0,               // 0000 0000
        CleanCorpses = 1,       // 0000 0001
        CleanWrecks = 2,        // 0000 0010
        CleanDuties = 3,        // 0000 0011
        PlaceObjects = 4,       // 0000 0100
        RedirectTraffic = 8,    // 0000 1000
        CopDuties = 8,          // 0000 1000
        HealPlayer = 16,        // 0001 0000
        ReanimatePed = 32,      // 0010 0000
        MedicDuties = 48,       // 0011 0000
        ReturnToVehicle = 64,   // 0100 0000
        IdleDuties = 64         // 0100 0000
    }
}