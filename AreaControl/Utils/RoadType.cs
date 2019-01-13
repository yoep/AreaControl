using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Utils
{
    /// <summary>
    /// Defines the road types on which can be searched.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum RoadType
    {
        All = 0,
        MajorRoadsOnly = 1
    }
}