using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;

namespace AreaControl.Instances
{
    public class Road : IPreviewSupport
    {
        #region Constructors

        internal Road(Vector3 position, Vector3 rightSide, Vector3 leftSide, IReadOnlyList<Lane> lanes, int numberOfLanes1, int numberOfLanes2,
            int junctionIndicator)
        {
            Position = position;
            RightSide = rightSide;
            LeftSide = leftSide;
            Lanes = lanes;
            NumberOfLanes1 = numberOfLanes1;
            NumberOfLanes2 = numberOfLanes2;
            JunctionIndicator = junctionIndicator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the center position of the road.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the right side position of the road.
        /// </summary>
        public Vector3 RightSide { get; }

        /// <summary>
        /// Get the left side position of the road.
        /// </summary>
        public Vector3 LeftSide { get; }

        /// <summary>
        /// Get the lanes of this road.
        /// </summary>
        public IReadOnlyList<Lane> Lanes { get; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes1 { get; }

        /// <summary>
        /// Get the total number of lanes.
        /// </summary>
        public int NumberOfLanes2 { get; }

        /// <summary>
        /// Get the junction indicator.
        /// </summary>
        public int JunctionIndicator { get; }

        /// <summary>
        /// Get or set the width of the road.
        /// </summary>
        public float Width
        {
            get { return Lanes.Select(x => x.Width).Sum(); }
        }

        /// <summary>
        /// Check if the road position is at a junction.
        /// </summary>
        public bool IsAtJunction => JunctionIndicator != 0;

        /// <summary>
        /// Check if the road goes in one direction (no opposite lane present).
        /// </summary>
        public bool IsSingleDirection => IsSingleDirectionRoad();

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive { get; private set; }

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            IsPreviewActive = true;
            GameFiber.StartNew(() =>
            {
                while (IsPreviewActive)
                {
                    Rage.Debug.DrawSphere(Position, 1f, Color.White);
                    Rage.Debug.DrawSphere(RightSide, 1f, Color.Blue);
                    Rage.Debug.DrawSphere(LeftSide, 1f, Color.Green);
                    GameFiber.Yield();
                }
            });
            foreach (var lane in Lanes)
            {
                lane.CreatePreview();
            }
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            IsPreviewActive = false;
            foreach (var lane in Lanes)
            {
                lane.DeletePreview();
            }
        }

        #endregion

        public override string ToString()
        {
            var message = Environment.NewLine + $"{nameof(Position)}: {Position}," +
                          Environment.NewLine + $"{nameof(RightSide)}: {RightSide}," +
                          Environment.NewLine + $"{nameof(LeftSide)}: {LeftSide}," +
                          Environment.NewLine + $"{nameof(NumberOfLanes1)}: {NumberOfLanes1}," +
                          Environment.NewLine + $"{nameof(NumberOfLanes2)}: {NumberOfLanes2}," +
                          Environment.NewLine + $"{nameof(JunctionIndicator)}: {JunctionIndicator}," +
                          Environment.NewLine + $"{nameof(IsAtJunction)}: {IsAtJunction}," +
                          Environment.NewLine + $"{nameof(IsSingleDirection)}: {IsSingleDirection}," +
                          Environment.NewLine + $"{nameof(Width)}: {Width}" +
                          Environment.NewLine + "--- Lanes ---" + Environment.NewLine;
            return Lanes.Aggregate(message, (current, lane) => current + (Environment.NewLine + lane)) + Environment.NewLine + "---";
        }

        private bool IsSingleDirectionRoad()
        {
            return NumberOfLanes1 == 0 || NumberOfLanes2 == 0;
        }

        /// <summary>
        /// Defines the lane information within the road.
        /// </summary>
        public class Lane : IPreviewSupport
        {
            #region Constructors

            public Lane(int number, float heading, Vector3 rightSide, Vector3 leftSide, Vector3 nodePosition, float width)
            {
                Number = number;
                Heading = heading;
                RightSide = rightSide;
                LeftSide = leftSide;
                NodePosition = nodePosition;
                Width = width;
                Position = CalculatePosition();
            }

            #endregion

            #region Properties

            /// <summary>
            /// Get the unique lane number.
            /// </summary>
            public int Number { get; }

            /// <summary>
            /// Get the heading of the lane.
            /// </summary>
            public float Heading { get; }

            /// <summary>
            /// Get the middle position of the lane.
            /// </summary>
            public Vector3 Position { get; }

            /// <summary>
            /// Get the right side start position of the lane.
            /// </summary>
            public Vector3 RightSide { get; }

            /// <summary>
            /// Get the left side start position of the lane.
            /// </summary>
            public Vector3 LeftSide { get; }

            /// <summary>
            /// Get the position of the node that was used for determining the heading of this lane.
            /// </summary>
            public Vector3 NodePosition { get; }

            /// <summary>
            /// Get the width of the lane.
            /// </summary>
            public float Width { get; }

            #endregion

            #region IPreviewSupport

            /// <inheritdoc />
            public bool IsPreviewActive { get; private set; }

            /// <inheritdoc />
            public void CreatePreview()
            {
                if (IsPreviewActive)
                    return;

                IsPreviewActive = true;
                GameFiber.StartNew(() =>
                {
                    var direction = MathHelper.ConvertHeadingToDirection(Heading);
                    var rightSideStart = FloatAboveGround(RightSide + direction * (2f * Number));
                    var rightSideEnd = FloatAboveGround(RightSide + direction * (2f * Number + 2f));
                    var leftSideStart = FloatAboveGround(LeftSide + direction * (2f * Number));
                    var leftSideEnd = FloatAboveGround(LeftSide + direction * (2f * Number + 2f));

                    while (IsPreviewActive)
                    {
                        Rage.Debug.DrawArrow(FloatAboveGround(Position), direction, Rotator.Zero, Width, Color.Red);
                        Rage.Debug.DrawLine(rightSideStart, rightSideEnd, Color.Blue);
                        Rage.Debug.DrawLine(leftSideStart, leftSideEnd, Color.Green);
                        GameFiber.Yield();
                    }
                });
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                IsPreviewActive = false;
            }

            #endregion

            public override string ToString()
            {
                return $"{nameof(Number)}: {Number}," + Environment.NewLine +
                       $"{nameof(Heading)}: {Heading}," + Environment.NewLine +
                       $"{nameof(RightSide)}: {RightSide}," + Environment.NewLine +
                       $"{nameof(LeftSide)}: {LeftSide}," + Environment.NewLine +
                       $"{nameof(NodePosition)}: {NodePosition}," + Environment.NewLine +
                       $"{nameof(Width)}: {Width}," + Environment.NewLine +
                       $"{nameof(IsPreviewActive)}: {IsPreviewActive}" + Environment.NewLine;
            }

            private Vector3 CalculatePosition()
            {
                var moveHeading = MathHelper.NormalizeHeading(Heading + 90f);
                var moveDirection = MathHelper.ConvertHeadingToDirection(moveHeading);

                return RightSide + moveDirection * (Width / 2);
            }

            private Vector3 FloatAboveGround(Vector3 position)
            {
                return position + Vector3.WorldUp * 0.25f;
            }
        }
    }
}