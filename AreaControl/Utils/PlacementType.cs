using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Utils
{
    /// <summary>
    /// Attachment placement places.
    /// https://pastebin.com/3pz17QGd
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum PlacementType
    {
        RightHand = 0x6F06,
        LeftHand = 0xEB95
    }
}