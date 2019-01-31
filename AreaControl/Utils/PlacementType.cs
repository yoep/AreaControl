using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Utils
{
    /// <summary>
    /// Attachment placement places.
    /// https://pastebin.com/3pz17QGd
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum PlacementType : uint
    {
        RightHand = 0xDEAD,
        LeftHand = 0x49D9
    }
}