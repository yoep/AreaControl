using System.Diagnostics.CodeAnalysis;
using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class PropUtils
    {
        public static Object CreateWand()
        {
            var model = new Model("prop_parking_wand_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateOpenNotebook()
        {
            var model = new Model("prop_notepad_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateNotebook()
        {
            var model = new Model("prop_notepad_02");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreatePencil()
        {
            var model = new Model("prop_pencil_01");
            return new Object(model, Vector3.Zero);
        }

        public static Object CreateSmallBlankCone(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_03"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateSmallConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_02"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateBigConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_01"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateLargeThinConeWithStripes(Vector3 position)
        {
            var instance = new Object(new Model("prop_mp_cone_04"), position);
            PlaceCorrectlyOnGround(instance);
            return instance;
        }

        public static Object CreateBarrier(Vector3 position)
        {
            return new Object(new Model("prop_ld_barrier_01"), position);
        }
        
        public static Object CreateWorkerBarrierArrowRight(Vector3 position)
        {
            return new Object(new Model("prop_mp_arrow_barrier_01"), position);
        }
        
        public static Object CreatePoliceDoNotCrossBarrier(Vector3 position)
        {
            return CreatePoliceDoNotCrossBarrier(position, 0f);
        }
        
        public static Object CreatePoliceDoNotCrossBarrier(Vector3 position, float heading)
        {
            var barrier = new Object(new Model("prop_barrier_work05"), position, heading);
            PlaceCorrectlyOnGround(barrier);
            return barrier;
        }

        public static bool PlaceCorrectlyOnGround(Object instance)
        {
            Assert.NotNull(instance, "instance cannot be null");
            return NativeFunction.Natives.PLACE_OBJECT_ON_GROUND_PROPERLY<bool>(instance);
        }

        public static void SetVisibility(Object instance, bool isVisible)
        {
            Assert.NotNull(instance, "instance cannot be null");
            if (!instance.IsValid())
                return;

            instance.Opacity = isVisible ? 1f : 0f;
        }

        public static void Remove(Object entity)
        {
            EntityUtils.Remove(entity);
        }
    }
}