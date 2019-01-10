using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using AreaControl.Utils.Tasks;
using Rage;

namespace AreaControl.Duties
{
    /// <summary>
    /// Duty for redirecting the traffic on the road.
    /// This duty will let the given ped walk to the given position and play the parking assistant animation.
    /// </summary>
    public class RedirectTrafficDuty : IDuty
    {
        private readonly IRage _rage;
        private readonly Vector3 _position;
        private readonly float _heading;
        private AnimationTaskExecutor _animationTaskExecutor;

        public RedirectTrafficDuty(Vector3 position, float heading)
        {
            _rage = IoC.Instance.GetInstance<IRage>();
            _position = position;
            _heading = heading;
        }

        /// <inheritdoc />
        public bool IsAvailable => true;

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            IsActive = true;
            _rage.NewSafeFiber(() =>
            {
                ped.IsBusy = true;
                ped.Instance.Tasks
                    .GoStraightToPosition(_position, 1f, _heading, 0f, 20000)
                    .WaitForCompletion();

                ped.Attach(PropUtil.CreateWand());
                _animationTaskExecutor = ped.PlayAnimation("amb@world_human_car_park_attendant@male@base", "base", AnimationFlags.Loop);
            }, typeof(RedirectTrafficDuty).Name);
        }

        /// <inheritdoc />
        public void Abort()
        {
            _animationTaskExecutor?.Abort();
        }
    }
}