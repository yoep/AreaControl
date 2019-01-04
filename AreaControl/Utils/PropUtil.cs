using Rage;

namespace AreaControl.Utils
{
    public static class PropUtil
    {
        public static Object CreateWand()
        {
            var model = new global::Rage.Model("prop_parking_wand_01");
            return new Object(model, Vector3.Zero);
        }
    }
}