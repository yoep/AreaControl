using AreaControl.Instances;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Utils
{
    public static class AnimationUtil
    {
        /// <summary>
        /// Attach props for animation and play the issue ticket.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <returns>Returns the animation task executor for this animation.</returns>
        public static AnimationTaskExecutor IssueTicket(ACPed ped)
        {
            ped.Attach(PropUtil.CreateNotebook(), PlacementType.LeftHand);
            ped.Attach(PropUtil.CreatePencil(), PlacementType.RightHand);
            return ped.PlayAnimation("veh@busted_low", "issue_ticket_cop", AnimationFlags.None);
        }
        
        /// <summary>
        /// Attach props an play the redirect traffic animation.
        /// </summary>
        /// <param name="ped">Set the ped that executes the animation.</param>
        /// <returns>Returns the animation task executor for this animation.</returns>
        public static AnimationTaskExecutor RedirectTraffic(ACPed ped)
        {
            ped.Attach(PropUtil.CreateWand(), PlacementType.RightHand);
            return ped.PlayAnimation("amb@world_human_car_park_attendant@male@base", "base", AnimationFlags.Loop);
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
    }
}