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
        #region Constructors

        internal Road(Vector3 position, IReadOnlyList<Lane> lanes, bool isAtJunction)
        {
            Position = position;
            Lanes = lanes;
            IsAtJunction = isAtJunction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the position of the road.
        /// </summary>
        public Vector3 Position { get; }

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

        /// <inheritdoc />
        public bool IsPreviewActive => Lanes.Select(x => x.IsPreviewActive).First();

        #endregion

        #region IPreviewSupport implementation

        /// <inheritdoc />
        public void CreatePreview()
        {
            foreach (var lane in Lanes)
            {
                lane.CreatePreview();
            }
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            foreach (var lane in Lanes)
            {
                lane.DeletePreview();
            }
        }

        #endregion

        public override string ToString()
        {
            var message = Environment.NewLine + $"{nameof(Position)}: {Position}," +
                          Environment.NewLine + $"{nameof(IsAtJunction)}: {IsAtJunction}," +
                          Environment.NewLine + $"{nameof(Width)}: {Width}" +
                          Environment.NewLine + "--- Lanes ---";
            return Lanes.Aggregate(message, (current, lane) => current + (Environment.NewLine + lane));
        }

        /// <summary>
        /// Defines the lane information within the road.
        /// </summary>
        public class Lane : IPreviewSupport
        {
            private Object _previewLeftSide;
            private Object _previewRightSide;
            private Object _previewDirection;

            public Lane(float heading, Vector3 rightSide, Vector3 leftSide, float width)
            {
                Heading = heading;
                RightSide = rightSide;
                LeftSide = leftSide;
                Width = width;
            }

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

                _previewLeftSide = PropUtil.CreateSmallBlankCone(LeftSide);
                _previewRightSide = PropUtil.CreateSmallConeWithStripes(RightSide);
                _previewDirection = PropUtil.CreateBigConeWithStripes(RightSide + MathHelper.ConvertHeadingToDirection(Heading) * 1f);
                PreviewUtil.TransformToPreview(_previewLeftSide);
                PreviewUtil.TransformToPreview(_previewRightSide);
                PreviewUtil.TransformToPreview(_previewDirection);
            }

            /// <inheritdoc />
            public void DeletePreview()
            {
                if (!IsPreviewActive)
                    return;

                _previewLeftSide.Dismiss();
                _previewLeftSide = null;
                _previewRightSide.Dismiss();
                _previewRightSide = null;
                _previewDirection.Dismiss();
                _previewDirection = null;
            }

            public override string ToString()
            {
                return $"{nameof(Heading)}: {Heading}," +
                       Environment.NewLine + $"{nameof(RightSide)}: {RightSide}," +
                       Environment.NewLine + $"{nameof(LeftSide)}: {LeftSide}," +
                       Environment.NewLine + $"{nameof(Width)}: {Width}," +
                       Environment.NewLine + $"{nameof(IsPreviewActive)}: {IsPreviewActive}";
            }
        }
    }
}