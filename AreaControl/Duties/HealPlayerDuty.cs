using AreaControl.Duties.Flags;
using AreaControl.Instances;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Duties
{
    public class HealPlayerDuty : AbstractDuty
    {
        #region Constructors 

        internal HealPlayerDuty(long id, ACPed ped) 
            : base(id, ped)
        {
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool IsAvailable => GetPlayer().Health < GetPlayer().MaxHealth;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => false;

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.HealPlayer;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.Medics;

        #endregion

        #region Functions

        /// <inheritdoc />
        protected override void DoExecute()
        {
            Logger.Info($"Executing heal player duty #{Id}");
            var player = GetPlayer();
            var walkToTask = Ped.WalkTo(player);

            // if aborted, warp ped to the player
            walkToTask.OnAborted += (sender, args) => Ped.Instance.Position = player.Position.Around2D(1f);
            walkToTask.WaitForCompletion(5000);

            var executor = AnimationUtils.GiveObject(Ped, PropUtils.CreateMedKit(Ped.Instance.Position));
            executor.OnCompletion += (o, eventArgs) => player.Health = player.MaxHealth;
            Logger.Info($"Completed heal player duty #{Id}");
        }

        private static Ped GetPlayer()
        {
            return Game.LocalPlayer.Character;
        }

        #endregion
    }
}