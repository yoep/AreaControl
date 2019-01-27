using System.Collections.Generic;
using AreaControl.AbstractionLayer;
using AreaControl.Menu;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Duties
{
    public class PlaceObjectsDuty : AbstractDuty
    {
        private readonly List<PlaceObject> _objects;
        private readonly ResponseCode _responseCode;
        private readonly IRage _rage;
        private readonly bool _placeFromHand;

        public PlaceObjectsDuty(long id, IEnumerable<PlaceObject> objects, ResponseCode responseCode, bool placeFromHand)
        {
            _objects = new List<PlaceObject>(objects);
            _responseCode = responseCode;
            _placeFromHand = placeFromHand;
            _rage = IoC.Instance.GetInstance<IRage>();
        }

        #region IDuty

        /// <inheritdoc />
        public override bool IsAvailable => _objects.Count > 0;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        #endregion

        #region AbstractDuty

        public override void Execute()
        {
            base.Execute();

            _rage.NewSafeFiber(() =>
            {
                foreach (var placeObject in _objects)
                {
                    var headingBehindObject = RoadUtil.OppositeHeading(placeObject.Heading);
                    var positionBehindCone = placeObject.Position + MathHelper.ConvertHeadingToDirection(headingBehindObject) * 0.8f;
                    var walkToExecutor = _responseCode == ResponseCode.Code2
                        ? Ped.WalkTo(positionBehindCone, placeObject.Heading)
                        : Ped.RunTo(positionBehindCone, placeObject.Heading);

                    if (IsAborted)
                        break;

                    var animationExecutor = walkToExecutor
                        .WaitForAndExecute(executor =>
                        {
                            _rage.LogTrivialDebug("Completed walk to place object for " + executor);
                            return AnimationUtil.PlaceDownObject(Ped, placeObject.Instance, _placeFromHand);
                        }, 20000)
                        .WaitForCompletion(2500);

                    if (IsAborted)
                        break;

                    _rage.LogTrivialDebug("Completed place object animation for " + animationExecutor);
                }

                CompleteDuty();
            }, "PlaceObjectsDuty.Execute");
        }

        public override void Abort()
        {
            base.Abort();
            _rage.NewSafeFiber(() => Ped.DeleteAttachments(), "PlaceObjectsDuty.Abort");
        }

        #endregion

        public class PlaceObject
        {
            private Object _instance;

            public PlaceObject(Vector3 position, float heading, System.Func<Vector3, float, Object> spawnInstance)
            {
                SpawnInstance = spawnInstance;
                Position = position;
                Heading = heading;
            }

            /// <summary>
            /// Get the position of the object to spawn.
            /// </summary>
            public Vector3 Position { get; }

            /// <summary>
            /// Get the heading of the object to spawn.
            /// </summary>
            public float Heading { get; }

            /// <summary>
            /// Get the instance to place down.
            /// </summary>
            public Object Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = SpawnInstance.Invoke(Position, Heading);

                    return _instance;
                }
            }

            private System.Func<Vector3, float, Object> SpawnInstance { get; }
        }
    }
}