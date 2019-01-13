using System;
using System.Collections.Generic;
using System.Linq;
using AreaControl.Utils;
using Rage;
using Object = Rage.Object;

namespace AreaControl.Instances
{
    public class Road : IPreviewSupport
    {
        private readonly List<Object> _previewObjects = new List<Object>();

        #region Constructors

        internal Road(Vector3 position, Vector3 rightSide, Vector3 leftSide, IReadOnlyList<Lane> lanes, bool isAtJunction, bool isSingleDirection)
        {
            Position = position;
            RightSide = rightSide;
            LeftSide = leftSide;
            Lanes = lanes;
            IsAtJunction = isAtJunction;
            IsSingleDirection = isSingleDirection;
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
        /// Get or set the width of the road.
        /// </summary>
        public float Width
        {
            get { return Lanes.Select(x => x.Width).Sum(); }
        }

        /// <summary>
        /// Check if the road position is at a junction.
        /// </summary>
        public bool IsAtJunction { get; }

        /// <summary>
        /// Check if the road goes in one direction (no opposite lane present).
        /// </summary>
        public bool IsSingleDirection { get; }

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObjects.Count > 0;

        /// <inheritdoc />
        public void CreatePreview()
        {
            _previewObjects.Add(PropUtil.CreateLargeThinConeWithStripes(Position));
            _previewObjects.Add(PropUtil.CreateLargeThinConeWithStripes(RightSide));
            _previewObjects.Add(PropUtil.CreateLargeThinConeWithStripes(LeftSide));
            _previewObjects.ForEach(PreviewUtil.TransformToPreview);
            foreach (var lane in Lanes)
            {
                lane.CreatePreview();
            }
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            _previewObjects.ForEach(PropUtil.Remove);
            _previewObjects.Clear();
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
                          Environment.NewLine + $"{nameof(IsAtJunction)}: {IsAtJunction}," +
                          Environment.NewLine + $"{nameof(IsSingleDirection)}: {IsSingleDirection}," +
                          Environment.NewLine + $"{nameof(Width)}: {Width}" +
                          Environment.NewLine + "--- Lanes ---" + Environment.NewLine;
            return Lanes.Aggregate(message, (current, lane) => current + (Environment.NewLine + lane)) + Environment.NewLine + "---";
        }

        /// <summary>
        /// Defines the lane information within the road.
        /// </summary>
        public class Lane : IPreviewSupport
        {
            private Object _previewLeftSide;
            private Object _previewRightSide;
            private Object _previewDirection;

            public Lane(int number, float heading, Vector3 rightSide, Vector3 leftSide, float width)
            {
                Number = number;
                Heading = heading;
                RightSide = rightSide;
                LeftSide = leftSide;
                Width = width;
            }

            /// <summary>
            /// Get the unique lane number.
            /// </summary>
            public int Number { get; }

            /// <summary>
            /// Get the heading of the lane.
            /// </summary>
            public float Heading { get; }

            /// <summary>
            /// Get the right side start position of the lane.
            /// </summary>
            public Vector3 RightSide { get; }

            /// <summary>
            /// Get the left side start position of the lane.
            /// </summary>
            public Vector3 LeftSide { get; }

            /// <summary>
            /// Get the width of the lane.
            /// </summary>
            public float Width { get; }

            /// <inheritdoc />
            public bool IsPreviewActive => _previewLeftSide != null;

            /// <inheritdoc />
            public void CreatePreview()
            {
                if (IsPreviewActive)
                    return;

                var offsetDirection = MathHelper.ConvertHeadingToDirection(Heading);

                _previewLeftSide = PropUtil.CreateSmallBlankCone(LeftSide + offsetDirection * (2f * Number));
                _previewRightSide = PropUtil.CreateSmallConeWithStripes(RightSide + offsetDirection * (2f * Number));
                _previewDirection = PropUtil.CreateBigConeWithStripes(RightSide + offsetDirection * (2f * Number + 2f));
                PreviewUtil.TransformToPreview(_previewLeftSide);
                PreviewUtil.TransformToPreview(_previewRightSide);
                PreviewUtil.TransformToPreview(_previewDirection);
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                PropUtil.Remove(_previewLeftSide);
                PropUtil.Remove(_previewRightSide);
                PropUtil.Remove(_previewDirection);
                _previewLeftSide = null;
                _previewRightSide = null;
                _previewDirection = null;
            }

            public override string ToString()
            {
                return $"{nameof(Number)}: {Number}," + Environment.NewLine +
                       $"{nameof(Heading)}: {Heading}," + Environment.NewLine +
                       $"{nameof(RightSide)}: {RightSide}," + Environment.NewLine +
                       $"{nameof(LeftSide)}: {LeftSide}," + Environment.NewLine +
                       $"{nameof(Width)}: {Width}," + Environment.NewLine +
                       $"{nameof(IsPreviewActive)}: {IsPreviewActive}" + Environment.NewLine;
            }
        }
    }
}