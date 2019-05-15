using System.Collections.Generic;
using AreaControl.Instances;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Utils
{
    public static class AnimationUtil
    {
        private const string PlaceDownObjectDictionary = "pickup_object";
        private const string PlaceDownObjectAnimation = "putdown_low";

        /// <summary>
        /// Attach props for animation and play the issue ticket.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <returns>Returns the animation task executor for this animation.</returns>
        public static AnimationTaskExecutor IssueTicket(ACPed ped)
        {
            ped.Attach(PropUtils.CreateNotebook(), PlacementType.LeftHand);
            ped.Attach(PropUtils.CreatePencil(), PlacementType.RightHand);
            return ped.PlayAnimation("veh@busted_low", "issue_ticket_cop", AnimationFlags.None);
        }

        /// <summary>
        /// Attach props an play the redirect traffic animation.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <returns>Returns the animation task executor for this animation.</returns>
        public static AnimationTaskExecutor RedirectTraffic(ACPed ped)
        {
            ped.Attach(PropUtils.CreateWand(), PlacementType.RightHand);
            var taskExecutor = ped.PlayAnimation("amb@world_human_car_park_attendant@male@base", "base", AnimationFlags.Loop);
            taskExecutor.OnCompletion += (sender, args) => ped.DeleteAttachments();
            return taskExecutor;
        }

        /// <summary>
        /// Talk to the police radio.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <returns>Returns the animation task executor for this animation.</returns>
        public static AnimationTaskExecutor TalkToRadio(ACPed ped)
        {
            return ped.PlayAnimation("random@arrests", "generic_radio_chatter", AnimationFlags.UpperBodyOnly);
        }

        public static AnimationTaskExecutor Investigate(ACPed ped)
        {
            return ped.PlayAnimation("amb@code_human_police_investigate@base", "base", AnimationFlags.None);
        }

        /// <summary>
        /// Place down the given entity.
        /// Attaches the entity before playing the animation, and removes the entity when the object is placed down.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <param name="entity">Set the entity to place down.</param>
        /// <param name="placeFromHand">Set if the must be showed in the hand of the ped.</param>
        /// <returns>Returns the animation executor.</returns>
        public static AnimationTaskExecutor PlaceDownObject(ACPed ped, Object entity, bool placeFromHand)
        {
            if (!entity.IsValid())
                return AnimationTaskExecutorBuilder.Builder()
                    .ExecutorEntities(new List<Ped> {ped.Instance})
                    .AnimationDictionary(PlaceDownObjectDictionary)
                    .AnimationName(PlaceDownObjectAnimation)
                    .IsAborted(true)
                    .Build();

            var originalPosition = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z);
            entity.MakePersistent();
            entity.Position = Vector3.Zero;

            if (placeFromHand)
            {
                ped.Attach(entity, PlacementType.RightHand);
            }

            var executor = ped.PlayAnimation(PlaceDownObjectDictionary, PlaceDownObjectAnimation, AnimationFlags.None);
            executor.OnCompletionOrAborted += (sender, args) =>
            {
                ped.DetachAttachments();

                if (!entity.IsValid())
                    return;

                entity.Position = originalPosition;
                PropUtils.PlaceCorrectlyOnGround(entity);
            };
            return executor;
        }
    }
}