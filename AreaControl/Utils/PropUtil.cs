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
        
        public static Object CreateNotebook()
        {
            var model = new Model("prop_notepad_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreatePencil()
        {
            var model = new Model("prop_pencil_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateSmallBlankCone(Vector3 position)
        {
            return new Object(new Model("prop_mp_cone_03"), position)
            {
                IsPersistent = false
            };
        }
        
        public static Object CreateSmallConeWithStripes(Vector3 position)
        {
            return new Object(new Model("prop_mp_cone_02"), position)
            {
                IsPersistent = false
            };
        }
        
        public static Object CreateBigConeWithStripes(Vector3 position)
        {
            return new Object(new Model("prop_mp_cone_01"), position)
            {
                IsPersistent = false
            };
        }
        
        public static Object CreateLargeThinConeWithStripes(Vector3 position)
        {
            return new Object(new Model("prop_mp_cone_04"), position)
            {
                IsPersistent = false
            };
        }

        public static void Remove(Object entity)
        {
            Assert.NotNull(entity, "entity cannot be null");
            entity.Dismiss();
            entity.Delete();
        }
    }
}