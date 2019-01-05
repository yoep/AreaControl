using Rage;

namespace AreaControl.Utils
{
    public static class PropUtil
    {
        public static Object CreateWand()
        {
            var model = new Model("prop_parking_wand_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateCone(Vector3 position)
        {
            return new Object(new Model("prop_mp_cone_03"), position);
        }
    }
}