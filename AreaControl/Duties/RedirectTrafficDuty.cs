using AreaControl.AbstractionLayer;
using AreaControl.Instances;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Duties
{
    public class RedirectTrafficDuty : IDuty
    {
        private readonly IRage _rage;
        private readonly Vector3 _position;
        private readonly float _heading;

        public RedirectTrafficDuty(Vector3 position, float heading)
        {
            _rage = IoC.Instance.GetInstance<IRage>();
            _position = position;
            _heading = heading;
        }

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public void Execute(ACPed ped)
        {
            _rage.NewSafeFiber(() =>
            {
                ped.Instance.Tasks
                    .GoStraightToPosition(_position, 1f, _heading, 0f, 20000)
                    .WaitForCompletion();
                
                ped.Attach(PropUtil.CreateWand());
                var animationDictionary = new AnimationDictionary("amb@world_human_car_park_attendant@male@base");
                ped.Instance.Tasks.PlayAnimation(animationDictionary, "base", 8.0f, AnimationFlags.Loop);
            }, typeof(RedirectTrafficDuty).Name);
        }
    }
}