using System.Linq;
using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public static class EntityUtils
    {
        /// <summary>
        /// Attach the given attachment to the given entity.
        /// </summary>
        /// <param name="attachment">Set the attachment entity.</param>
        /// <param name="target">Set the target entity.</param>
        /// <param name="placement">Set the place to attach the attachment to at the target.</param>
        public static void AttachEntity(Entity attachment, Entity target, PedBoneId placement)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            Assert.NotNull(target, "target cannot be null");
            var boneId = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(target, (int) placement);

            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(attachment, target, boneId, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, false, false, false, 2, 1);
        }

        /// <summary>
        /// Detach the given attachment from it's entity.
        /// </summary>
        /// <param name="attachment">Set the attachment.</param>
        public static void DetachEntity(Entity attachment)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            NativeFunction.Natives.DETACH_ENTITY(attachment, false, false);
        }

        /// <summary>
        /// Remove the given entity from the game world.
        /// </summary>
        /// <param name="entity">Set the entity to remove.</param>
        public static void Remove(Entity entity)
        {
            Assert.NotNull(entity, "entity cannot be null");
            if (!entity.IsValid())
                return;

            entity.IsPersistent = false;
            entity.Dismiss();
            entity.Delete();
        }

        /// <summary>
        /// Clean the given area from entities (e.g. vehicles, objects, etc.)
        /// </summary>
        /// <param name="position">Set the position to clean.</param>
        /// <param name="radius">Set the area around the position to clean.</param>
        public static void CleanArea(Vector3 position, float radius)
        {
            Assert.NotNull(position, "position cannot be null");
            var entitiesToClean = World
                .GetEntities(position, radius,
                    GetEntitiesFlags.ConsiderGroundVehicles | GetEntitiesFlags.ConsiderAllObjects | GetEntitiesFlags.ExcludePlayerPed)
                .Where(x => x.IsValid())
                .Where(x => x != Game.LocalPlayer.LastVehicle);

            foreach (var entity in entitiesToClean)
            {
                Remove(entity);
            }
        }
    }
}