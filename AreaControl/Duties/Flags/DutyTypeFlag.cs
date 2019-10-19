using System;
using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Duties.Flags
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DutyTypeFlag
    {
        None = 0,               // 00000000
        CleanCorpses = 1,       // 00000001
        CleanWrecks = 2,        // 00000010
        CleanDuties = 3,        // 00000011
        PlaceObjects = 4,       // 00000100
        RedirectTraffic = 8,    // 00001000
        CopDuties = 8,          // 00001000
        HealPlayer = 16,        // 00010000
        ReanimatePed = 32,      // 00100000
        MedicDuties = 48,       // 00110000
        ReturnToVehicle = 64    // 01000000
    }
}