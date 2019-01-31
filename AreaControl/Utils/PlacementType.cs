using System.Diagnostics.CodeAnalysis;

namespace AreaControl.Utils
{
    /// <summary>
    /// Attachment placement places (= bone ID in GTA V).
    /// https://pastebin.com/3pz17QGd
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum PlacementType : uint
    {
        RightHand = 0x6F06,
        LeftHand = 0x49D9
    }
}