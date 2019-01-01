using System;
using Rage;

namespace AreaControl.Model
{
    public class Road
    {
        public Road(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        public Road(Vector3 position, Vector3 rightSide, Vector3 leftSide, float heading, float width)
        {
            Position = position;
            RightSide = rightSide;
            LeftSide = leftSide;
            Heading = heading;
            Width = width;
        }

        /// <summary>
        /// Get the position of the road.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get or set the right side start position.
        /// </summary>
        public Vector3 RightSide { get; set; }

        /// <summary>
        /// Get or set the left side start position.
        /// </summary>
        public Vector3 LeftSide { get; set; }

        /// <summary>
        /// Get the heading of the road.
        /// </summary>
        public float Heading { get; }

        /// <summary>
        /// Get or set the width of the road.
        /// </summary>
        public float Width { get; set; }

        public override string ToString()
        {
            return
                Environment.NewLine + $"{nameof(Position)}: {Position}, " +
                Environment.NewLine + $"{nameof(RightSide)}: {RightSide}, " +
                Environment.NewLine + $"{nameof(LeftSide)}: {LeftSide}," +
                Environment.NewLine + $" {nameof(Heading)}: {Heading}, " +
                Environment.NewLine + $"{nameof(Width)}: {Width}";
        }
    }
}