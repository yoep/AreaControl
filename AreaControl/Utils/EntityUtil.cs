using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public static class EntityUtil
    {
        /// <summary>
        /// Attach the given attachment to the given entity.
        /// </summary>
        /// <param name="attachment">Set the attachment entity.</param>
        /// <param name="target">Set the target entity.</param>
        /// <param name="attachmentPlacement">Set the place to attach the attachment to at the target.</param>
        public static void AttachEntity(Entity attachment, Entity target, PlacementType attachmentPlacement)
        {
            Assert.NotNull(attachment, "attachment cannot be null");
            Assert.NotNull(target, "target cannot be null");
            var boneId = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(target, (int) attachmentPlacement);
            
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
    }
}