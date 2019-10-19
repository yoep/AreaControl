using System.Collections.Generic;
using AreaControl.Duties.Flags;
using AreaControl.Menu.Response;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Duties
{
    public class PlaceObjectsDuty : AbstractDuty
    {
        private readonly List<PlaceObject> _objects;
        private readonly ResponseCode _responseCode;
        private readonly bool _placeFromHand;

        #region Constructors

        internal PlaceObjectsDuty(long id, IEnumerable<PlaceObject> objects, ResponseCode responseCode, bool placeFromHand) 
            : base(id)
        {
            _objects = new List<PlaceObject>(objects);
            _responseCode = responseCode;
            _placeFromHand = placeFromHand;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool IsAvailable => _objects.Count > 0;

        /// <inheritdoc />
        public override bool IsRepeatable => true;

        /// <inheritdoc />
        public override bool IsMultipleInstancesAllowed => true;

        /// <inheritdoc />
        public override DutyTypeFlag Type => DutyTypeFlag.PlaceObjects;

        /// <inheritdoc />
        public override DutyGroupFlag Groups => DutyGroupFlag.All;

        #endregion

        #region AbstractDuty

        protected override void DoExecute()
        {
            Logger.Info($"Executing place objects duty #{Id}");
            foreach (var placeObject in _objects)
            {
                var directionPlayerToObject = GetPlayerDirectionToObject(placeObject);
                var positionBehindObject = placeObject.Position - directionPlayerToObject * 0.8f;
                var walkToExecutor = _responseCode == ResponseCode.Code2
                    ? Ped.WalkTo(positionBehindObject, placeObject.Heading)
                    : Ped.RunTo(positionBehindObject, placeObject.Heading);

                if (IsAborted)
                    break;

                var animationExecutor = walkToExecutor
                    .WaitForAndExecute(executor =>
                    {
                        Rage.LogTrivialDebug("Completed walk to place object for " + executor);
                        return AnimationUtils.PlaceDownObject(Ped, placeObject.Instance, _placeFromHand);
                    }, 20000)
                    .WaitForCompletion(2500);

                if (IsAborted)
                    break;

                Rage.LogTrivialDebug("Completed place object animation for " + animationExecutor);
            }
            Logger.Info($"Completed place objects duty #{Id}");
        }

        public override void Abort()
        {
            base.Abort();
            Rage.NewSafeFiber(() => Ped.DeleteAttachments(), "PlaceObjectsDuty.Abort");
        }

        #endregion

        #region Functions

        private static Vector3 GetPlayerDirectionToObject(PlaceObject placeObject)
        {
            return MathHelper.ConvertHeadingToDirection(placeObject.Heading);
        }

        #endregion

        public class PlaceObject
        {
            private Object _instance;

            public PlaceObject(Vector3 position, float heading, System.Func<Object> spawnInstance)
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
                        _instance = SpawnInstance.Invoke();

                    return _instance;
                }
            }

            private System.Func<Object> SpawnInstance { get; }
        }
    }
}